# C# ASTC Encoder

ASTC Encoder는 ARM의 공식 ASTC 인코더(`astcenc`) 라이브러리를 활용하여 이미지를 ASTC 포맷으로 변환하는 C# 콘솔 애플리케이션입니다. 이 프로젝트는 고품질의 텍스처 압축을 필요로 하는 게임 및 그래픽 애플리케이션을 지원하기 위해 설계되었습니다.

---

## 특징

- **ARM 공식 ASTC 인코더 통합**: `astcenc` 라이브러리를 사용하여 정확하고 효율적인 ASTC 압축 제공
- **사용자 정의 블록 크기**: 4x4, 6x6 등 다양한 블록 크기 지원
- **품질 설정 가능**: 빠른 압축부터 고품질 압축까지 다양한 품질 옵션 제공
- **간단한 CLI 인터페이스**: 명령줄에서 손쉽게 이미지 변환 가능
- **MIT License**: 자유로운 사용 및 수정 가능

---

## 설치 방법

1. **필수 구성 요소**
   - .NET 6 이상 설치
   - ARM의 [ASTC Encoder](https://github.com/ARM-software/astc-encoder) 빌드 후 `astcenc.dll` 생성 및 실행 디렉토리에 배치

2. **프로젝트 클론**
```bash
git clone https://github.com/<your-repo>/astc-image-encoder.git
cd astc-image-encoder
```

3. **빌드**
```bash
dotnet build -c Release
```

4. **실행 파일 위치**
- 빌드된 실행 파일은 `bin/Release/net6.0/` 디렉토리에 생성됩니다.

---

## 사용 방법

### 기본 사용법
```bash
AstcEncoder <input_image> <output_file> [block_size] [quality]
```

### 예제

1. 기본 설정으로 PNG 파일을 ASTC로 변환 (블록 크기: 4x4, 품질: medium)

```bash
AstcEncoder input.png output.astc
```

2. 블록 크기를 6x6, 품질을 high로 설정하여 변환

```bash
AstcEncoder input.png output.astc 6x6 high
```

### 옵션 설명

| 옵션          | 설명                                                                 |
|---------------|----------------------------------------------------------------------|
| `input_image` | 변환할 입력 이미지 경로 (PNG, BMP 등 지원)                           |
| `output_file` | 생성될 ASTC 파일 경로                                               |
| `block_size`  | 압축 블록 크기 (예: 4x4, 6x6, 기본값: 4x4)                           |
| `quality`     | 압축 품질 (fast, medium, high, thorough, exhaustive; 기본값: medium) |

---

## 라이선스

이 프로젝트는 MIT License 하에 배포됩니다. 자세한 내용은 [LICENSE](./LICENSE)를 참조하세요.

---

## 참고 자료

- [ARM ASTC Encoder GitHub Repository](https://github.com/ARM-software/astc-encoder)
- [ASTC Format Overview](https://developer.arm.com/documentation/100140/latest)

---

이 프로젝트에 대한 피드백과 기여를 환영합니다! 😊


