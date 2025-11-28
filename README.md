# ImgView

**WPF (.NET 8) で構築した軽量画像ビューア / 簡易電子書籍ビューア（ZIP / EPUB対応）です。**  
ReactiveProperty を用いた MVVM 構成で、プレイリスト・全画面・見開き表示・ZIP内画像の高速表示に対応しています。

📝 GitHub 上のアプリを入手してビルド・実行する方法については、下記のブログ記事で解説しています：
https://maywork.net/computer/github-app-clone-build-install/

---

## ✨ 主な特徴

### 📁 画像ファイルの閲覧
- PNG / JPEG / JPG / BMP / WEBP に対応  
- ZIP / EPUB をそのままドロップすると中の画像を展開して閲覧可能  
- 横長画像は単ページ、縦長画像は見開きで自動レイアウト  
- 非同期ローディングで高速スクロール

### 📂 プレイリスト機能
- ドラッグ&ドロップで複数ファイルを追加  
- 並び替え（上へ / 下へ）  
- 削除・クリア・保存・読み込み  
- 編集モード（F5）

### 🏃 画面操作
- **クリック操作でページ送り**
  - 左クリック：次の画像へ  
  - 右クリック：前の画像へ  
- **F11**：全画面 / 通常を切り替え  
- マウス操作は非同期版（Async）でスムーズな更新

### ⚡ 高速化ポイント
- `ImageCacheManager` によるメモリキャッシュ  
- ZIP/EPUBストリーム読み込みのメモリコピー最適化  
- 常に BGRA32 + DPI96 に統一し高パフォーマンスを実現  
- 非同期画像合成 (`PlaceOnCanvasImageAsync`) で見開きを高速生成

---

## 🖥️ 動作環境

- Windows 10 / Windows 11  
- .NET 8  
- WPF アプリケーション

---

## 🚀 使用方法

### 1. GitHub からクローン

```powershell
git clone https://github.com/あなた/ImgView.git
cd ImgView
dotnet run
