===============================================================================
【タイトル】 comeconv
【ファイル】 comeconv.exe
【作成月日】 2021/06/18
【著 作 者】 nnn-revo2012
【開発環境】 Microsoft Windows 10
             Microsoft Visual Studio Community 2017
【動作環境】 Microsoft Windows 10 / Windows 8.1 / Windows 7
             .NET Framework 4.7.2
【推奨環境】 Microsoft Windows 10
【配布形態】 フリーウェア
【Web Site】 https://github.com/nnn-revo2012/comeconv
【 連絡先 】 要望やバグ報告等はgithubまで
             その他　nnn_revo2012@yahoo.co.jp　
===============================================================================

■説明
・ニコニコ生放送のコメントや動画を変換するツールです。
・GUI(Windows Forms)使用。
・ニコ生新配信録画ツール（仮などでダウンロードした動画やコメントをさきゅばすで合成するための
  各種変換をおこないます
・そのほかのコメントや動画も行えるようにする予定。

■インストール方法
適当なフォルダにzipファイルの中のファイルを全て解凍してください。解凍したらその中のcomeconv.exe を実行してください。
※ダウンロード時や実行時にウイルスやマルウェアの警告が出る可能性があります。
  当方でウイルスチェックは行っておりますがあらかじめご了承ください。

■アンインストール方法
アンインストールの際は comeconv.exe の入っているフォルダごと削除してください。

■使用方法
1.comeconv.exeを起動する。
2.さきゅばす変換タブが表示されます。
　初期値では絵文字は空白、流量調整コメ（うすい文字）は通常文字として表示、ギフトとエモーションは画面下に
　固定表示されます。
3.運営コメやNGワードにしたいコメントはNGワードの「編集」ボタンをクリックしてメモ帳で追加してください。
4.動画変換はニコ生新配信録画ツール(仮 などで録画した場合の .tsファイルをさきゅばすで扱えるように
　変換するものです(FFmpegでtsファイルからmp4/flvに変換してるだけです)。
5.真中の右側の「Save」ボタンを押すと設定が保存されます。
6.画面の緑色の部分に動画ファイルやコメントファイルをドラッグ＆ドロップすると変換されます。

■動作環境
.Net Framework 4.7.2以降が必要です。Windows 10では標準でインストールされています。
https://support.microsoft.com/ja-jp/topic/windows-%E7%94%A8%E3%81%AE-microsoft-net-framework-4-7-2-web-%E3%82%A4%E3%83%B3%E3%82%B9%E3%83%88%E3%83%BC%E3%83%A9%E3%83%BC-dda5cddc-b85e-545d-8d4a-d213349b7775

■動作確認ソフト
・Saccubus（さきゅばす）1.67.2.11
https://github.com/Saccubus/Saccubus1.x/releases
・動画ファイルおよびコメントファイル
  - ニコ生新配信録画ツール（仮
  - livedl
  - NCV (コメントファイルのみ)
  ※ニコニコ動画の動画ファイル、コメントファイルは未検証です

■免責事項
本ソフトウェアを利用して発生した如何なる損害について著作者は一切の責任を負いません。
また著作者はバージョンアップ、不具合修正の義務を負いません。

■ライセンス関係
・comeconv
https://github.com/nnn-revo2012/comeconv
Copyright (c) 2021 nnn-revo2012
Released under the MIT License

・Json.NET
https://www.newtonsoft.com/json
Copyright (c) 2007 James Newton-King
Released under the MIT License

・FFmpeg
https://www.ffmpeg.org/
https://github.com/FFmpeg/FFmpeg/
Copyright (c) 2000-2021 the FFmpeg developers
GNU General Public License v3.0

■更新履歴
2021/06/18　Version 0.0.1.05
初期バージョンリリース
