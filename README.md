<div align="center">
  <img src="https://via.placeholder.com/850x200/239120/FFFFFF?text=Data_Manager+Project+Banner" width="100%" alt="Project Banner"/>
  <br/><br/>

  # 🏎️ Data_Manager
  **자율주행 통합 데이터 관리 플랫폼 (Donkeycar + C# WinForms + WSL)**

  <br/>

  <img src="https://img.shields.io/badge/C%23-239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#"/>
  <img src="https://img.shields.io/badge/Python-3670A0?style=for-the-badge&logo=python&logoColor=ffdd54" alt="Python"/>
  <img src="https://img.shields.io/badge/Ubuntu_WSL-E95420?style=for-the-badge&logo=ubuntu&logoColor=white" alt="Ubuntu"/>
  <img src="https://img.shields.io/badge/Donkeycar-4D4D4D?style=for-the-badge&logo=smart&logoColor=white" alt="Donkeycar"/>
  <br/><br/>
</div>

<hr/>

## 💡 프로젝트 개요 (Overview)
본 프로젝트는 오픈소스 자율주행 플랫폼인 동키카(Donkeycar)의 시뮬레이터 환경을 활용하여, 인공지능(AI) 차량의 주행 데이터를 수집, 정제, 학습, 검증하는 통합 시스템입니다. 기존 웹 기반 UI의 성능적/기능적 한계를 극복하기 위해 기획된 **C# WinForms 기반의 독자적 인터페이스**입니다. 

* **🛡️ 비파괴 편집 보장:** 원본 데이터는 최대한 보존하고, 편집물은 안전하게 별도 분리 저장합니다.
* **📊 다차원 비교 분석:** 동일한 이미지에 대해 실제 사용자의 주행 데이터와 AI 모델의 판단값을 프레임 단위로 1:1 비교합니다.

<br/>

## 👥 작업 분담 (Team 11)
| 담당자 | 담당 영역 | 주요 역할 및 업무 범위 |
|:---:|:---:|---|
| **민승호** | **Manager** | C# WinForms 기반 UI/UX 설계, 로직 개발, 이미지 렌더링 최적화 |
| **최상훈** | **Pilot** | C# WinForms 기반 UI/UX 설계, 로직 개발, 전반적인 기능 점검 및 수정|
| **장상** | **Trainer** | 트레이너 UI 구축, AI 학습 및 WSL 연동, 모델 리스트 관리 및 테스트·검증 |
| **이호준** | **Trainer** | 트레이너 UI 구축, AI 학습 및 WSL 연동, 모델 리스트 관리 및 테스트·검증 |

<br/>

## 🚀 핵심 기능 (Core Features)
프로그램은 데이터의 라이프사이클(준비 ➡️ 학습 ➡️ 테스트)에 맞춰 3개의 메인 화면으로 구성됩니다.

### 🖥️ 1. Manager (데이터 탐색 및 비파괴 정제)
> **주행 데이터 불러오기, 프레임 탐색, 비파괴 편집, 여러 tub 병합, edit_tubs 저장**

* **직관적인 탐색:** 이미지와 steering/throttle 값을 동기화하여 표시하며, 슬라이드 재생과 프레임 이동을 원활하게 지원합니다.
* **안전한 가공:** 원본 파일을 보존(비파괴 편집)하며 편집/병합된 데이터는 `edit_tubs`에 완전히 독립된 형태로 저장됩니다.

### 🤖 2. Trainer (AI 학습 및 라이프사이클 관리)
> **데이터 로드, AI 학습 실행, 자율주행 시작, 모델 리스트 및 파일 관리**

* **원클릭 학습 및 주행 제어:** '데이터 로드' 후 버튼 클릭만으로 고유한 모델명이 자동 생성되며, WSL 환경에서 `train.py`가 백그라운드 실행됩니다. 학습 완료된 모델을 리스트에서 선택해 별도의 파일 탐색 없이 즉시 자율주행(`manage.py drive`)을 시작할 수 있습니다.
* **직관적인 모델 관리:** 학습된 모델(.h5)은 리스트에 자동 추가됩니다. 삭제, 이름 변경(중복 검사 및 .h5 확장자 자동 추가) 기능을 통해 체계적으로 관리할 수 있습니다.

### 📊 3. Pilot (예측 데이터 추출 및 오차 분석)
> **모델 불러오기, 시뮬레이터/주행 테스트, AI 판단값 추출, 실제 조작값과 비교**

* **1:1 프레임 매칭 검증:** 모델이 예측한 데이터를 추출하여 원본 레코드의 `user_angle`, `user_throttle` 값과 프레임 인덱스 기준으로 정밀 비교합니다.
* **위험 구간 탐지:** 오차가 큰 구간, 조향 급변 구간을 시각화하고 분석 결과를 `predictions.csv/json` 등으로 내보냅니다.

<br/>

## 🛠️ 기술 스택 (Tech Stack)
| 분류 | 사용 기술 |
|---|---|
| **Client UI** | C# WinForms (Windows 환경) |
| **Backend/Bridge** | WSL (Windows Subsystem for Linux), PowerShell, Process |
| **Linux OS** | Ubuntu 22.0.4 |
| **AI Environment** | Python, Conda (`e2e_env`), Donkeycar Framework |
| **Data Format** | Tub 구조, JSON, CSV |

<br/>

## 📁 시스템 아키텍처 및 폴더 구조
<details>
<summary><b>👉 상세 디렉토리 구조 펼쳐보기</b></summary>
<div markdown="1">

```text
~/mycar/
 ├─ data/                   # 원본 시뮬레이터 주행 데이터 보존 (비파괴 원칙)
 ├─ edit_tubs/              # Manager 화면에서 재가공/병합된 데이터셋 산출물
 ├─ models/                 # Trainer 화면에서 생성된 학습 모델 (.h5) 및 사용 데이터 메타데이터
 ├─ predictions/            # Pilot 화면의 AI 예측 결과 및 CSV 비교 분석 리포트
 └─ log/                    # 앱 실행 기록, 학습 로그, 시스템 오류 로그 통합