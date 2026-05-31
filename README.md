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

## 👥 작업 분담 계획 (Team 11)
| 담당 | 주요 역할 |
|:---:|---|
| **민승호(팀장) & 최상훈** | C# WinForms 기반 UI/UX 설계, 비파괴 데이터 정제 로직 개발, 이미지 렌더링 최적화, WSL 연동 공통 함수 설계 |
| **장상 & 이호준** | 시뮬레이터 주행 데이터 10,000 장 이상 확보, 학습 파라미터 튜닝, 모델 주행 테스트, 결과 데이터 검증 |
| **공통** | 기능 통합 테스트, 오류 로그 수집, 발표 자료/보고서 작성, 최종 시연 준비 |

<br/>

## 🚀 핵심 기능 (Core Features)
프로그램은 데이터의 라이프사이클(준비 ➡️ 학습 ➡️ 테스트)에 맞춰 3개의 메인 화면으로 구성됩니다.

### 🖥️ 1. Manager (데이터 탐색 및 비파괴 정제)
> **주행 데이터 불러오기, 프레임 탐색, 비파괴 편집, 여러 tub 병합, edit_tubs 저장**

<div align="center">
  [gif1: Manager 기능 구동 화면]
</div>

* **직관적인 탐색:** 이미지와 steering/throttle 값을 동기화하여 표시하며, 슬라이드 재생과 프레임 이동을 원활하게 지원합니다.
* **안전한 가공:** 원본 파일을 보존(비파괴 편집)하며 편집/병합된 데이터는 `edit_tubs`에 완전히 독립된 형태로 저장됩니다.

### 🤖 2. Trainer (AI 학습 및 라이프사이클 관리)
> **학습 데이터 선택, train.py 실행, 학습 로그 저장, 모델 및 메타데이터 관리**

<div align="center">
  [gif2: Trainer 기능 구동 화면]
</div>

* **원클릭 학습 제어:** 정제된 데이터를 선택 후, C# 버튼으로 WSL의 `e2e_env`를 활성화하고 `train.py`를 즉시 백그라운드 실행합니다.
* **체계적인 관리:** 학습된 모델은 `mycar/log`에 저장되며, 별칭 지정과 리스트업을 통해 사용된 데이터 목록과 함께 완벽하게 추적됩니다.

### 📊 3. Pilot (예측 데이터 추출 및 오차 분석)
> **모델 불러오기, 시뮬레이터/주행 테스트, AI 판단값 추출, 실제 조작값과 비교**

<div align="center">
  [gif3: Pilot 기능 구동 화면]
</div>

* **1:1 프레임 매칭 검증:** 모델이 예측한 데이터를 추출하여 원본 record의 `user_angle`, `user_throttle` 값과 프레임 인덱스 기준으로 정밀 비교합니다.
* **위험 구간 탐지:** 오차가 큰 구간, 조향 급변 구간을 시각화하고 모든 분석 결과를 `predictions.csv/json` 등으로 내보냅니다.

<br/>

## 🛠️ 기술 스택 (Tech Stack)
| 분류 | 사용 기술 |
|---|---|
| **Client UI** | C# WinForms (Windows 환경) |
| **Backend/Bridge** | WSL (Windows Subsystem for Linux), PowerShell, Process |
| **Linux OS** | Ubuntu |
| **AI Environment** | Python, Conda (`e2e_env`), Donkeycar Framework |
| **Data Format** | Tub 구조, JSON, CSV |

<br/>

## 📁 시스템 아키텍처 및 폴더 구조
Data_Manager는 C# UI와 **WSL 연동 함수, Python helper를 안정적으로 분리**하는 데 아키텍처의 초점을 맞췄습니다. 모든 산출물은 호환성을 위해 리눅스의 `~/mycar` 폴더를 기준으로 관리됩니다.

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