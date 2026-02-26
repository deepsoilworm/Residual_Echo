# 개발 원칙

## 협업 원칙
- 내 파트만 작업한다. 담당 범위는 그때그때 지시한다.
- 다른 파트 코드는 수정하지 않는다. 필요 시 인터페이스/이벤트로 연결한다.
- 클래스/public 메서드에 `/// <summary>` XML 주석 필수 (다른 개발자가 읽을 수 있도록)
- 파트 간 의존은 인터페이스로 연결, 직접 참조 최소화

## 코딩 규칙

### 금지 사항
- 런타임 `new GameObject()`, `AddComponent<>()` 금지 (스폰 예외)
- 하드코딩 금지 → 상수 클래스로 관리
- 매직넘버 금지 → `[SerializeField]` 또는 ScriptableObject
- 리플렉션 금지 → public 프로퍼티/메서드 제공
- `Resources.Load` 지양 → `[SerializeField]` Inspector 연결

### UI 텍스트 원칙
- UI 텍스트는 처음 만들 때 영어로 작성한다 (로컬라이제이션은 나중에)

### 구조 원칙
- Unity 개발의 국룰을 따른다.
- 에디터/런타임 분리 (씬 구성은 에디터에서, 런타임은 게임 로직만)
- 컴포넌트 기반: 1 MonoBehaviour = 1 책임
- 데이터: ScriptableObject로 분리
- 통신: C# event 또는 UnityEvent (직접 참조 최소화)
- 상태 관리: State Pattern
- 자식 오브젝트로 따라다니기 (HP바, 이펙트 등은 Transform 부모-자식으로 자동 추적)
- UI 요소는 프리팹에 미리 구성
