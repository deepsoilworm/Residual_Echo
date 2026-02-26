# Contributing Guide - Residual Echo

## 브랜치 전략

| 브랜치 | 용도 | 비고 |
|--------|------|------|
| `main` | 배포용 | 직접 커밋 금지 |
| `develop` | 개발 통합 | PR 머지 대상 |
| `feature/*` | 기능 개발 | `feature/이름/설명` |
| `hotfix/*` | 긴급 버그 | `hotfix/이름/설명` |
| `bugfix/*` | 일반 버그 | `bugfix/이름/설명` |

### 브랜치 네이밍
```
feature/Beaver/player-move
feature/DOC/skill-system
bugfix/Beaver/creature-detection
hotfix/DOC/crash-on-load
```

## 커밋 메시지

```
[type]: 한 줄 요약
```

| 타입 | 의미 |
|------|------|
| `feat` | 기능 추가 |
| `fix` | 버그 수정 |
| `refactor` | 구조 개선 |
| `chore` | 설정, 빌드 |
| `test` | 테스트 |

### 예시
```
feat: 플레이어 이동 시스템 추가
fix: 보스 패턴 무한 반복 버그 수정
refactor: 스킬 시스템 모듈 분리
chore: .gitignore 업데이트
```

## PR 규칙

1. **develop ← feature 브랜치**로 PR 생성
2. 최소 **1명 리뷰** (형식적이어도 OK)
3. PR 제목은 커밋 메시지 컨벤션과 동일하게
4. 컴파일 에러 없는 상태에서만 머지

## 폴더 구조

```
Assets/
├── Animations/     # AnimatorController, 클립
│   └── Creature/
├── Materials/      # 머티리얼
│   └── Creature/
├── Models/         # FBX, 3D 모델
│   └── Creature/
├── Textures/       # 텍스처 (albedo, normal, metallic 등)
│   └── Creature/
├── Prefabs/        # 프리팹
├── Scenes/         # 씬 파일
├── ScriptableObjects/  # SO 에셋
│   ├── Creature/
│   └── Player/
├── Scripts/        # C# 스크립트
│   ├── Common/     # 공용 (Constants, Interfaces, Events)
│   ├── Player/     # 플레이어 (Controller, Interaction, Flashlight)
│   ├── Creature/   # 크리처 AI (AI, Detection, Data)
│   └── Editor/     # 에디터 도구
└── Settings/       # URP, 렌더 파이프라인 설정
```

## 작업 영역 분담

| 담당 | 영역 |
|------|------|
| Beaver | Player/, Creature/, Common/ |
| 닥양 | Item/, Level/, Inventory/ |
| 협의 | GameManager/, UI/ |
