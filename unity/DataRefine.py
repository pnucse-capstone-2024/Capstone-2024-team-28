import pandas as pd
import numpy as np
from scipy.spatial import ConvexHull
import json

# 엑셀 파일에서 데이터 로드
file_path = r'C:\Users\admin\Desktop\graduationproj\lidardata\6207_clean_ver.xlsx'  # 실제 경로를 입력
data = pd.read_excel(file_path, sheet_name=0)

# 각도(angle)와 거리(range) 데이터를 2D 좌표로 변환
angles = np.deg2rad(data['Angle'])
ranges = data['Range']

# 극좌표계를 직교좌표계로 변환 (x, y 좌표)
x_coords = ranges * np.cos(angles)
y_coords = ranges * np.sin(angles)

# 2D 좌표를 모아둔 배열
points = np.column_stack((x_coords, y_coords))

# Convex Hull 계산
hull = ConvexHull(points)

# Convex Hull 꼭짓점 추출
hull_vertices = points[hull.vertices]

# 기둥 사이의 겹침을 방지하기 위해 x와 z 좌표에 비율 적용
scaling_factor = 50.0  # x, z 좌표를 스케일링할 비율 (값을 조정하면서 확인 가능)
scaled_hull_vertices = hull_vertices * scaling_factor  # 좌표를 비례하여 확장

# 꼭짓점 간 연결 정보 및 기둥 데이터 생성
pillar_data_list = []
for i, vertex in enumerate(scaled_hull_vertices):
    # 기둥 데이터 생성
    pillar = {
        "x": float(vertex[0]),    # 스케일링된 x 좌표
        "y": 0.0,                 # 기둥의 높이 기반 설정 (바닥에 배치)
        "z": float(vertex[1]),    # 스케일링된 z 좌표
        "height": 5.0             # 기둥 높이 (원하는 높이로 설정)
    }

    # 연결 정보 (다음 꼭짓점과 연결)
    connections = [{"to_pillar_index": (i + 1) % len(scaled_hull_vertices), "wall_thickness": 0.5}]

    # 각 기둥 데이터에 연결 정보 추가
    pillar_data_list.append({
        "pillar": pillar,
        "connections": connections
    })

# JSON 형식으로 저장
output_json_path = r'C:\Users\admin\Desktop\graduationproj\lidardata\pillars_with_walls.json'  # 실제 경로 설정
with open(output_json_path, 'w') as json_file:
    json.dump({"pillars": pillar_data_list}, json_file, indent=4)

print("기둥과 벽 정보를 JSON 파일로 성공적으로 저장했습니다.")

