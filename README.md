# B4Thesis-VR: 実空間ペダリング連動型VRサイクリングシステム

卒業研究として開発した、実際に自転車を漕ぐ動作とVR空間内の移動を連動させるシステムです。
WebカメラとArUcoマーカーによるペダル動作検出、TCP通信、Meta Quest VRヘッドセットを組み合わせ、
国土交通省PLATEAU（台東区3D都市モデル）内を実際に漕いで移動する体験を実現しています。

## システム概要

```
[自転車ペダル] → [WebカメラでArUco検出] → [Python TCPサーバー]
                                                      ↓ 座標データ送信
[Meta Quest VRヘッドセット] ← [Unity (XR Interaction Toolkit)]
           ↑
    PLATEAU台東区3D都市空間内を移動
```

### 動作フロー

1. 自転車ペダルにArUcoマーカーを取り付ける
2. Pythonスクリプト（`mark.py`）がWebカメラでマーカーのY座標を50Hzでトラッキング
3. 検出座標をTCP経由でUnityへリアルタイム送信
4. Unityがマーカーの上下運動からペダルのRPMを算出し、前進速度に変換
5. プレイヤーがVRヘッドセット越しにPLATEAU 3D都市内を走行

## 使用技術

| カテゴリ | 技術 |
|---|---|
| VRエンジン | Unity 2022 (URP), XR Interaction Toolkit 2.6.5 |
| VRデバイス | Meta Quest (Oculus XR Plugin 4.5.2) |
| 3D都市データ | Project PLATEAU / PLATEAU SDK for Unity（台東区 2024年データ） |
| 動作検出 | Python + OpenCV + ArUco (DICT_4X4_50) |
| 通信 | TCP ソケット（Python サーバー ↔ Unity C# 非同期クライアント） |
| UI | TextMeshPro, Unity UGUI |

## ディレクトリ構成

```
B4Thesis-VR/
├── Assets/
│   ├── Script/                      # C#スクリプト（本体ロジック）
│   │   ├── NetworkCoordinateClient.cs   # TCP受信クライアント（async/await）
│   │   ├── CameraMoveByPitch.cs         # RPM算出・前進速度制御
│   │   ├── NeckSnapTurn.cs              # 首の向きによるスナップターン
│   │   ├── CheckPointManager.cs         # チェックポイント管理・ゴール演出
│   │   ├── CheckPointTrigger.cs         # チェックポイント接触判定
│   │   ├── UIDirectionIndicator.cs      # 次目標への方向矢印UI
│   │   └── CameraMove.cs                # キーボード移動（デバッグ用）
│   ├── Resources/                   # Pythonサーバー
│   │   ├── mark.py                      # ArUcoマーカートラッキング + TCPサーバー本体
│   │   ├── markleft.txt / markright.txt # マーカーしきい値設定
│   │   └── pyproject.toml               # Python依存関係定義
│   ├── Scenes/
│   │   └── SampleScene.unity            # メインシーン
│   ├── PLATEAU-SDK-for-Unity/       # PLATEAUアセット
│   ├── POLYGON city pack/           # 3D都市補完アセット
│   └── StreamingAssets/.PLATEAU/    # 台東区CityGML・3D建物データ
├── Packages/manifest.json           # Unityパッケージ依存関係
└── ProjectSettings/
```

## セットアップ

### 必要環境

- Unity 2022.3 LTS
- Python 3.14+
- Meta Quest（Link接続またはワイヤレス）
- WebカメラおよびArUcoマーカー（4x4辞書、ID:20）

### Pythonサーバーの起動

```bash
cd Assets/Resources
uv run mark.py
# または
pip install opencv-python numpy
python mark.py
```

### Unity側の設定

1. `SampleScene.unity` を開く
2. `NetworkCoordinateClient` コンポーネントの `Server IP` と `Port`（デフォルト: `127.0.0.1:5000`）を確認
3. `CameraMoveByPitch` コンポーネントで `Upper Threshold` / `Lower Threshold` をマーカーの実測値に合わせて調整
4. Playモードを開始

## 主要スクリプト解説

### `CameraMoveByPitch.cs` — ペダリング速度制御

ペダルの上死点・下死点をしきい値で検出し、通過間隔からRPMを算出。
目標RPMに対する比率でVR空間内の前進速度をリニアに制御します。
マーカーロスト時はタイムアウト付きで速度をフェードアウトさせることで、
瞬時停止による違和感を防いでいます。

### `NetworkCoordinateClient.cs` — 非同期TCP通信

`async/await` + `Task.Run` による非同期受信でUnityのメインスレッドをブロックしません。
接続断時は2秒後に自動再接続し、受信バッファの分割パケット対策として
改行区切りによるメッセージ境界処理を実装しています。

### `NeckSnapTurn.cs` — VR酔い対策

VRヘッドセットの首回転角が閾値（デフォルト60°）を超えると、
XR Origin全体を30°単位でスナップ回転させます。
スムーズ回転による酔いを防ぎながら、直感的な方向転換を実現します。

## 工夫した点

- **ゼロコピー設計**: Python→Unityの座標転送はCSV形式のテキストストリームで実装し、シリアライズコストを最小化
- **二段階しきい値**: 上死点・下死点それぞれに独立したしきい値を設けることでマーカーノイズに対するロバスト性を確保
- **Lerp減速**: `Mathf.Lerp`による速度補間で急加速・急停車感を排除し自然な乗り心地を実現
- **PLATEAUデータ活用**: 実在する台東区の3D建物・道路データを使用することで現実感のある走行体験を提供

## スクリーンショット / デモ
### マーカ読み取りの様子
<img width="500" height="500" alt="Image" src="https://github.com/user-attachments/assets/c8900b77-9252-4efd-8aa8-c5f6def9714e" />

### 被験者の様子
<img width="500" height="500" alt="Image" src="https://github.com/user-attachments/assets/b1bc0f31-e7aa-465e-8d31-f07a7c3048ad" />

### 使用したシーン
<img width="500" height="500" alt="Image" src="https://github.com/user-attachments/assets/645cdc13-0c26-410a-aa93-ce61e440ac10" />

### 被験者の視点
<img width="500" height = "500" alt="Image" src="https://github.com/user-attachments/assets/d7e51ac6-e7a2-4788-bac6-e822b1395ac0" />

## ライセンス

- 3D都市モデル（PLATEAU）: [Project PLATEAU 利用規約](https://www.mlit.go.jp/plateau/use-policy/)に準拠
- POLYGON City Pack: Unity Asset Store ライセンス

---

*2025年度 卒業研究*
