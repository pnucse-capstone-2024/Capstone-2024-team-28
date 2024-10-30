import numpy as np
import cv2
import cv2.aruco as aruco
import csv
import asyncio
import websockets
import base64
import ssl
import ffmpeg
import json

# SSL ���� (������ ���� - �׽�Ʈ �뵵)
ssl_context = ssl.create_default_context()
ssl_context.check_hostname = False
ssl_context.verify_mode = ssl.CERT_NONE

# Camera calibration parameters
camera_matrix1 = np.array([
    [491.113868, 0, 342.807878],
    [0, 490.403182, 245.694960],
    [0, 0, 1]
])

camera_matrix2 = np.array([
    [491.113868, 0, 342.807878],
    [0, 490.403182, 245.694960],
    [0, 0, 1]
])

camera_matrix3 = np.array([
    [491.113868, 0, 342.807878],
    [0, 490.403182, 245.694960],
    [0, 0, 1]
])

dist_coeffs = np.array([0.121950, -0.208711, 0.000476, 0.014367, 0.0])

# Marker size
marker_length = 0.18

# ������ URI for position data
websocket_uri_position = #"�������ּ�1"
# ������ URI 1, 2, 3
websocket_uri1 = #"�������ּ�2"
websocket_uri2 = #"�������ּ�3"
websocket_uri3 = #"�������ּ�4"


# ������ �̵�� �޽���
media_message = "video/h264;codecs=avc1.42E01E;width=320;height=240;framerate=60;bitrate=6000000;"

# initial position
initial_positions = {0: None, 1: None, 2: None}
initial_rotations = {0: None, 1: None, 2: None}

# puttext xy
print_2d = {0: (10,20), 1: (10,40), 2: (10,60)}
# �� ī�޶��� ȸ��, ��ȯ ��� - ���� ���ؾ� �մϴ�. (ī�޶� Ķ���극�̼��� ����)
# ī�޶� 1
R1 = np.eye(3)  # Camera1's rotation (Identity if camera1 is in world origin)
T1 = np.zeros((3, 1))  # Camera1's translation (0, 0, 0)
# ī�޶� 2
R2 = np.array([[0.9998477, -0.0174524, 0.002],  # Replace with actual rotation matrix
               [0.0174524, 0.9998477, 0],
               [-0.002, 0, 1]])
T2 = np.array([[0.5], [0.0], [0.0]])  # Example: Camera2 is 0.5 meters right of Camera1
# ī�޶� 3
R3 = np.array([[0.9998477, 0.0174524, 0],  # Replace with actual rotation matrix
               [-0.0174524, 0.9998477, 0],
               [0, 0, 1]])
T3 = np.array([[-0.5], [0.0], [0.0]])  # Example: Camera3 is 0.5 meters left of Camera1

# ���������� JSON �޽��� ������
async def send_position_data(websocket, marker_id, world_coords):
    # ���� �����͸� JSON �������� ��ȯ (int�� ��ȯ�Ͽ� ����ȭ �����ϰ� ��)
    position_data = {
        "marker_id": int(marker_id),  # numpy.intc�� int�� ��ȯ
        "world_x": float(world_coords[0]),  # numpy.float�� float�� ��ȯ
        "world_y": float(world_coords[1]),
        "world_z": float(world_coords[2])
    }
    # JSON ���ڿ��� ��ȯ
    json_string = json.dumps(position_data)

    # Base64�� ���ڵ�
    encoded_message =json_string.encode('utf-8')

    # ���������� ���ڵ��� �޽��� ����
    await websocket.send(encoded_message)

async def send_frame_h264(websocket, frame):
    # FFmpeg�� ����Ͽ� H264�� ���ڵ� (�ӵ� ����ȭ)
    process = (
        ffmpeg
        .input('pipe:0', format='rawvideo', pix_fmt='bgr24', s='{}x{}'.format(frame.shape[1], frame.shape[0]))
        .output('pipe:1', vcodec='libx264', format='h264', g=10, pix_fmt='yuv420p', threads=4, 
                preset='ultrafast',  # �ӵ��� ���̱� ���� ultrafast preset ���
                tune='zerolatency')  # ���� �ð��� ���̱� ���� zerolatency ���
        .run_async(pipe_stdin=True, pipe_stdout=True, pipe_stderr=True)
    )

    # ������ �����͸� FFmpeg�� �����Ͽ� ���ڵ�
    process.stdin.write(frame.tobytes())
    process.stdin.close()

    # ���ڵ��� ������ ��������
    h264_frame = b""
    while True:
        data = process.stdout.read(1024)  # 1KB�� �о����
        if not data:
            break
        h264_frame += data

    # ���̳ʸ� �����͸� ���������� ����
    await websocket.send(h264_frame)
        


# �� ī�޶��� �������� ����� ���ϴ� �Լ� (3D ��ǥ -> 2D ��ǥ)
def get_projection_matrix(camera_matrix, R, T):
    RT = np.hstack((R, T))  # Combine rotation and translation
    return np.dot(camera_matrix, RT)

P1 = get_projection_matrix(camera_matrix1, R1, T1)
P2 = get_projection_matrix(camera_matrix2, R2, T2)
P3 = get_projection_matrix(camera_matrix3, R3, T3)

# draw_axix
def draw_axis1(img, camera_matrix1, dist_coeffs, rvec, tvec, length):
    axis = np.float32([[length, 0, 0], [0, length, 0], [0, 0, length], [0, 0, 0]]).reshape(-1, 3)
    imgpts, _ = cv2.projectPoints(axis, rvec, tvec, camera_matrix1, dist_coeffs)
    imgpts = np.int32(imgpts).reshape(-1, 2)

    # Draw the base of the axis
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[0]), (0, 0, 255), 3)  # X axis in red
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[1]), (0, 255, 0), 3)  # Y axis in green
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[2]), (255, 0, 0), 3)  # Z axis in blue

    return img

def draw_axis2(img, camera_matrix2, dist_coeffs, rvec, tvec, length):
    axis = np.float32([[length, 0, 0], [0, length, 0], [0, 0, length], [0, 0, 0]]).reshape(-1, 3)
    imgpts, _ = cv2.projectPoints(axis, rvec, tvec, camera_matrix2, dist_coeffs)
    imgpts = np.int32(imgpts).reshape(-1, 2)

    # Draw the base of the axis
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[0]), (0, 0, 255), 3)  # X axis in red
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[1]), (0, 255, 0), 3)  # Y axis in green
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[2]), (255, 0, 0), 3)  # Z axis in blue

    return img

def draw_axis3(img, camera_matrix3, dist_coeffs, rvec, tvec, length):
    axis = np.float32([[length, 0, 0], [0, length, 0], [0, 0, length], [0, 0, 0]]).reshape(-1, 3)
    imgpts, _ = cv2.projectPoints(axis, rvec, tvec, camera_matrix3, dist_coeffs)
    imgpts = np.int32(imgpts).reshape(-1, 2)

    # Draw the base of the axis
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[0]), (0, 0, 255), 3)  # X axis in red
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[1]), (0, 255, 0), 3)  # Y axis in green
    img = cv2.line(img, tuple(imgpts[3]), tuple(imgpts[2]), (255, 0, 0), 3)  # Z axis in blue

    return img


def rvec_to_euler(rvec):
    R, _ = cv2.Rodrigues(rvec)
    sy = np.sqrt(R[0, 0] ** 2 + R[1, 0] ** 2)
    singular = sy < 1e-6

    if not singular:
        x = np.arctan2(R[2, 1], R[2, 2])
        y = np.arctan2(-R[2, 0], sy)
        z = np.arctan2(R[1, 0], R[0, 0])
    else:
        x = np.arctan2(-R[1, 2], R[1, 1])
        y = np.arctan2(-R[2, 0], sy)
        z = 0

    return np.array([x, y, z])

def triangulate_marker(corners1, corners2, corners3, P1, P2, P3):
    # �� ī�޶󿡼� ��Ŀ �߽����� ����
    corner1_center = np.mean(corners1[0], axis=0)
    corner2_center = np.mean(corners2[0], axis=0)
    corner3_center = np.mean(corners3[0], axis=0)

    # ������ 2D ��ǥ�� ���� ��ǥ�� ��ȯ (1 �߰�)
    points1 = np.array([corner1_center[0], corner1_center[1], 1.0])
    points2 = np.array([corner2_center[0], corner2_center[1], 1.0])
    points3 = np.array([corner3_center[0], corner3_center[1], 1.0])

    # �� ī�޶� ���� �ﰢ���� ���� (P1, P2)
    points4D_hom12 = cv2.triangulatePoints(P1, P2, points1[:2], points2[:2])
    
    # �� ��° ī�޶�� P1�� �ﰢ���� ���� (P1, P3)
    points4D_hom13 = cv2.triangulatePoints(P1, P3, points1[:2], points3[:2])

    # �� ����� ���� ��ǥ���� 3D�� ��ȯ
    points3D_12 = points4D_hom12[:3] / points4D_hom12[3]
    points3D_13 = points4D_hom13[:3] / points4D_hom13[3]

    # �� 3D ��ǥ�� ����� ���Ͽ� ���� ��Ȯ�� 3D ��ġ�� ��ȯ
    points3D = (points3D_12 + points3D_13) / 2

    return points3D  # X, Y, Z ��ǥ ��ȯ

async def keep_alive(websocket):
    while True:
        try:
            await websocket.ping()  # �ֱ������� ping �޽��� ����
            await asyncio.sleep(20)  # 10�ʸ��� ping
        except websockets.ConnectionClosed:
            break
        
async def detect_aruco_markers(max_translation=1.0):
    global initial_position, initial_marker_id
    cap = cv2.VideoCapture(0)
    cap2 =cv2.VideoCapture(1)
    cap3 =cv2.VideoCapture(2)

    if not cap.isOpened():
        print("Error: Could not open video.")
        return

    if not cap2.isOpened():
        print("Error: Could not open video.")
        return
    
    if not cap3.isOpened():
        print("Error: Could not open video.")
        return
    
    
    # ARUCO dictionary and detector parameters
    aruco_dict = aruco.getPredefinedDictionary(aruco.DICT_ARUCO_ORIGINAL)
    aruco_params = aruco.DetectorParameters()

    # save csv name
    csv_file_name = 'marker_positions.csv'
    file  = open(csv_file_name, mode ="w", newline='', encoding='utf-8')

    writer = csv.writer(file)
    writer.writerow(
    ['Frame', 'Marker ID', 'Relative X', 'Relative Y', 'Relative Z', 'Relative Roll', 'Relative Pitch',
     'Relative Yaw', 'World X', 'World Y', 'World Z'])

    frame_count = 0
    frame2_count = 0
    frame3_count = 0
    frame4_count = 0
    async with websockets.connect(websocket_uri1, ssl=ssl_context, ping_interval=60, ping_timeout=9999) as websocket1, \
               websockets.connect(websocket_uri2, ssl=ssl_context, ping_interval=60, ping_timeout=9999) as websocket2, \
               websockets.connect(websocket_uri3, ssl=ssl_context, ping_interval=60, ping_timeout=9999) as websocket3, \
               websockets.connect(websocket_uri_position, ssl=ssl_context, ping_interval=60, ping_timeout=9999) as websocket:

        # ������ ���� �� �̵�� ���� �޽��� ����
        await websocket1.send(media_message)
        await websocket2.send(media_message)
        await websocket3.send(media_message)
        print("Media message sent to all websockets")
        
        # keep-alive �½�ũ ����
        asyncio.create_task(keep_alive(websocket1))
        asyncio.create_task(keep_alive(websocket2))
        asyncio.create_task(keep_alive(websocket3))   
        
        while True:
        
            ret1, frame = cap.read()
            ret2, frame2 = cap2.read()
            ret3, frame3 = cap3.read()
        
            if not (ret1 and ret2 and ret3):
                break

            # ARUCO marker detection1
            corners, ids, rejected = aruco.detectMarkers(frame, aruco_dict, parameters=aruco_params)

            frame_count += 1

            if ids is not None:
                rvecs, tvecs, _ = aruco.estimatePoseSingleMarkers(corners, marker_length, camera_matrix1, dist_coeffs)
                aruco.drawDetectedMarkers(frame, corners, ids)

                for i in range(len(ids)):
                    marker_id = ids[i][0]

                    if marker_id in initial_positions.keys():
                        frame = draw_axis1(frame, camera_matrix1, dist_coeffs, rvecs[i], tvecs[i], marker_length * 0.5)

                        tvec_magnitude = np.linalg.norm(tvecs[i])

                        # Set the initial position for the marker if it's not already set
                        if initial_positions[marker_id] is None:
                            initial_positions[marker_id] = tvecs[i].flatten()
                            initial_rotations[marker_id] = rvecs[i].flatten()
                          #  print(f"Initial Position Set: {initial_positions[marker_id]} for Marker ID {marker_id}")

                        # Calculate relative movement for the marker
                        relative_position = tvecs[i].flatten() - initial_positions[marker_id]
                        #print(f"Marker ID: {marker_id}")
                        #print(f"Relative Position: {relative_position}")
                        relative_rotation = rvec_to_euler(rvecs[i])

                        '''
                        position_2d, _ = cv2.projectPoints(np.array([relative_position]), rvecs[i], tvecs[i], camera_matrix,
                                                           dist_coeffs)
                        position_2d = tuple(position_2d[0].ravel().astype(int))
                        '''
                        # Write results to CSV file
                        writer.writerow(
                            [frame_count, marker_id, relative_position[0], relative_position[1], relative_position[2],
                             relative_rotation[0], relative_rotation[1], relative_rotation[2]])

                        frame = cv2.putText(frame, f"Mark {marker_id} : x:{relative_position[0]:.4f} y:{relative_position[1]:.4f} z:{relative_position[2]:.4f} rx:{relative_rotation[0]:.4f} ry:{relative_rotation[1]:.4f} rz:{relative_rotation[2]:.4f}", print_2d[marker_id], cv2.FONT_HERSHEY_SIMPLEX, 0.5,(255, 0, 0), 2)

            # ARUCO marker detection2
            corners2, ids2, rejected2 = aruco.detectMarkers(frame2, aruco_dict, parameters=aruco_params)

            frame2_count += 1

            if ids2 is not None:
                rvecs, tvecs, _ = aruco.estimatePoseSingleMarkers(corners2, marker_length, camera_matrix1, dist_coeffs)
                aruco.drawDetectedMarkers(frame2, corners2, ids2)

                for i in range(len(ids2)):
                    marker_id2 = ids2[i][0]

                    if marker_id2 in initial_positions.keys():
                        frame2 = draw_axis1(frame2, camera_matrix2, dist_coeffs, rvecs[i], tvecs[i], marker_length * 0.5)

                        tvec_magnitude = np.linalg.norm(tvecs[i])

                        # Set the initial position for the marker if it's not already set
                        if initial_positions[marker_id2] is None:
                            initial_positions[marker_id2] = tvecs[i].flatten()
                            initial_rotations[marker_id2] = rvecs[i].flatten()
                          #  print(f"Initial Position Set: {initial_positions[marker_id2]} for Marker ID {marker_id2}")

                        # Calculate relative movement for the marker
                        relative_position = tvecs[i].flatten() - initial_positions[marker_id2]
                       # print(f"Marker ID: {marker_id2}")
                      #  print(f"Relative Position: {relative_position}")
                        relative_rotation = rvec_to_euler(rvecs[i])

                        '''
                        position_2d, _ = cv2.projectPoints(np.array([relative_position]), rvecs[i], tvecs[i], camera_matrix,
                                                           dist_coeffs)
                        position_2d = tuple(position_2d[0].ravel().astype(int))
                        '''
                        # Write results to CSV file
                        writer.writerow(
                            [frame2_count, marker_id2, relative_position[0], relative_position[1], relative_position[2],
                            relative_rotation[0], relative_rotation[1], relative_rotation[2]])

                        frame2 = cv2.putText(frame2, f"Mark {marker_id2} : x:{relative_position[0]:.4f} y:{relative_position[1]:.4f} z:{relative_position[2]:.4f} rx:{relative_rotation[0]:.4f} ry:{relative_rotation[1]:.4f} rz:{relative_rotation[2]:.4f}", print_2d[marker_id2], cv2.FONT_HERSHEY_SIMPLEX, 0.5,(255, 0, 0), 2)
            # ARUCO marker detection3
            corners3, ids3, rejected3 = aruco.detectMarkers(frame3, aruco_dict, parameters=aruco_params)

            frame3_count += 1

            if ids3 is not None:
                rvecs, tvecs, _ = aruco.estimatePoseSingleMarkers(corners3, marker_length, camera_matrix3, dist_coeffs)
                aruco.drawDetectedMarkers(frame3, corners3, ids3)

                for i in range(len(ids3)):
                    marker_id3 = ids3[i][0]

                    if marker_id3 in initial_positions.keys():
                        frame3 = draw_axis1(frame3, camera_matrix3, dist_coeffs, rvecs[i], tvecs[i], marker_length * 0.5)

                        tvec_magnitude = np.linalg.norm(tvecs[i])

                        # Set the initial position for the marker if it's not already set
                        if initial_positions[marker_id3] is None:
                            initial_positions[marker_id3] = tvecs[i].flatten()
                            initial_rotations[marker_id3] = rvecs[i].flatten()
                          #  print(f"Initial Position Set: {initial_positions[marker_id3]} for Marker ID {marker_id3}")

                        # Calculate relative movement for the marker
                        relative_position = tvecs[i].flatten() - initial_positions[marker_id3]
                       # print(f"Marker ID: {marker_id3}")
                       # print(f"Relative Position: {relative_position}")
                        relative_rotation = rvec_to_euler(rvecs[i])

                        '''
                        position_2d, _ = cv2.projectPoints(np.array([relative_position]), rvecs[i], tvecs[i], camera_matrix,
                                                           dist_coeffs)
                        position_2d = tuple(position_2d[0].ravel().astype(int))
                        '''
                        # Write results to CSV file
                        writer.writerow(
                            [frame3_count, marker_id3, relative_position[0], relative_position[1], relative_position[2],
                             relative_rotation[0], relative_rotation[1], relative_rotation[2]])

                        frame3 = cv2.putText(frame3, f"Mark {marker_id3} : x:{relative_position[0]:.4f} y:{relative_position[1]:.4f} z:{relative_position[2]:.4f} rx:{relative_rotation[0]:.4f} ry:{relative_rotation[1]:.4f} rz:{relative_rotation[2]:.4f}", print_2d[marker_id3], cv2.FONT_HERSHEY_SIMPLEX, 0.5,(255, 0, 0), 2)
            frame4_count += 1
            if ids is not None and ids2 is not None and ids3 is not None:
                for i in range(len(ids)):
                    marker_id4 = ids[i][0]
                
                    # �ﰢ������ ���� �� ī�޶��� ���� ��Ŀ�� ã�ƾ� ��
                    if marker_id4 in ids2 and marker_id4 in ids3:
                        index2 = np.where(ids2 == marker_id4)[0][0]
                        index3 = np.where(ids3 == marker_id4)[0][0]
                    
                        # �� ī�޶󿡼� ��Ŀ�� �ڳ� ���� ��������
                        corners_cam1 = corners[i]
                        corners_cam2 = corners2[index2]
                        corners_cam3 = corners3[index3]

                        # ���� ��ǥ�迡�� ��Ŀ�� 3D ��ǥ ����
                        world_coords = triangulate_marker(corners_cam1, corners_cam2, corners_cam3, P1, P2, P3)
                        print(f"Marker ID: {marker_id4} World Coordinates: {world_coords}")       
                        writer.writerow(
                            [frame4_count, marker_id4, '', '', '', '', '', '',  # Empty placeholders for relative data
                             world_coords[0], world_coords[1], world_coords[2]])  # 3D world coordinates  # 3D world coordinates
                        # ���������� 3D ��ǥ ����
                        await send_position_data(websocket, marker_id4, world_coords)
                        
            # Display the result frame
            cv2.imshow('Camera1', frame)
            cv2.imshow('Camera2', frame2)
            cv2.imshow('Camera3', frame3)
            
            # ���������� �� ������ ����
            await send_frame_h264(websocket1, frame)
            await send_frame_h264(websocket2, frame2)
            await send_frame_h264(websocket3, frame3)
            # Break loop on 'q' key press
            if cv2.waitKey(1) & 0xFF == ord('q'):
                break
            
        file.close()
        cap.release()
        cap2.release()
        cap3.release()
        cv2.destroyAllWindows()        

asyncio.run(detect_aruco_markers())




