# 클라우드 기반 로봇 관제 및 모니터링 시스템

### 1. 프로젝트 소개
#### 1.1. 배경 및 필요성
최근 산업 자동화와 스마트 기술의 발전으로 로봇의 활용이 급증하고 있습니다. 제조업, 물류, 의료 등 다양한 분야에서 로봇은 중요한 역할을 수행하고 있으며, 이러한 로봇의 효율적인 관리와 모니터링이 필수적입니다. 특히 여러 대의 로봇이 동시에 작업하는 환경에서는 각 로봇의 상태와 위치를 실시간으로 파악하고 관리하는 것이 매우 중요합니다.

#### 1.2. 목표 및 주요 내용
본 프로젝트의 목표는 클라우드 기반의 실시간 로봇 모니터링 및 제어 시스템을 개발하는 것입니다.

주요 기능:
- Aruco Marker와 컴퓨터 비전 기술을 활용한 정확한 실시간 로봇 위치 추적
- LiDAR 데이터를 활용한 3D 환경 모니터링 및 시각화
- 클라우드 기반의 실시간 비디오 스트리밍 및 원격 모니터링
- ESP32를 활용한 저비용, 고효율의 로봇 제어
- 웹 기반 사용자 인터페이스를 통한 직관적인 로봇 제어 및 모니터링

### 2. 상세설계
#### 2.1. 시스템 구성도
[시스템 구성도 이미지 추가 필요]

#### 2.2. 사용 기술
**Hardware**
- ESP32 마이크로컨트롤러
- 웹캠 (Aruco Marker 인식용)
- T-mini-pro LiDAR 센서
- WAVE ROVER(4WD ROVER)

**Backend**
- Python (OpenCV, LiDAR 데이터 처리)
- AWS EC2 (클라우드 서버)
- WebSocket 서버

**Frontend**
- HTML5/CSS3/JavaScript
- Unity/WebGL (3D 환경 시각화)

**Robot Control**
- Arduino IDE
- C++ (ESP32)
- ROS (Robot Operating System)

### 3. 설치 및 사용 방법
1. 환경 설정
```bash
# OpenCV 설치
pip install opencv-python

# Unity 설치
[Unity Hub 다운로드 링크]

# ESP32 개발 환경 설정
[Arduino IDE 설치 및 ESP32 보드 매니저 추가 방법]

2. 실행 방법
```bash
# 위치 추적 시스템 실행
python tracking_system.py

# 웹 서버 실행
node server.js

# Unity 프로젝트 빌드 및 실행
[Unity 프로젝트 빌드 방법]
```

### 4. 소개 및 시연 영상

주요 기능 데모:
- 실시간 로봇 위치 추적
- 3D 환경 시각화
- 원격 제어 시스템
- 통합 모니터링 대시보드

https://www.youtube.com/watch?v=JxSF9VgbEFM&list=PLFUP9jG-TDp-CVdTbHvql-WoADl4gNkKj&index=28

### 5. 팀 소개

이경원

- 역할: 컴퓨터 비전 기반 실시간 로봇 위치 추적 시스템 개발
- 담당: Aruco Marker 위치 추적, 실시간 데이터 처리

김범모

- 역할: LiDAR 기반 3D 환경 시각화 시스템 개발
- 담당: Unity 3D 환경 구현, LiDAR 데이터 처리

최정혜

- 역할: 클라우드 시스템 개발 및 통합, ESP32 기반 로봇 제어
- 담당: 웹 인터페이스 개발, 클라우드 인프라 구축, 로봇 제어 시스템 구현
