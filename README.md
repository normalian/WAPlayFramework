# Windows Azure 上で PlayFramework アプリケーションを動作！
Cloud Services を利用して、Windows Azure 上で PlayFramework のアプリケーションを動作させます。

## 利用手順
* play dist コマンドを利用してアプリケーションパッケージを作成します。
* 作成した PlayFramework のアプリケーション内部に start.bat を入れ込む
* ストレージサービス上に JRE, PlayFramework ランタイム, PlayFramework アプリケーションを配置します

### PlayFramework アプリケーションの構成(例は helloworld-1.0.zip の中身)
```
└─helloworld-1.0
    │  README
    │  start
    │  start.bat
    │  
    ├─lib
    │      
    └─logs
```

### start.bat
```
echo off
setlocal
set p=%~dp0
set p=%p:\=/%
java %* -cp "%p%/lib/*;" play.core.server.NettyServer %p%
```

