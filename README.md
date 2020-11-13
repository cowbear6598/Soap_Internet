## 含有其他插件 (Include Other Plugins)
- More Effective Coroutines [FREE]
- Json.net

如有專案中已經有這兩個插件，可以先行刪除專案中的，以免造成衝突問題，因為會要求刪除專案中的插件，所以會時刻注意保持在最新版本。

If there are these plugins in your project, please delete them to avoid conflict.
If there is a new version, I will update immediately.

## 使用說明

> Soap -> Internet -> MysqlSetting 設定需要用到的 http[s]://[domain]

- MysqlManager.OnConnetFail 綁定事件，當網路連線出錯時呼叫
- MysqlManager.RunRequestAPIByPost 要求 POST 方法
- MysqlManager.RunRequestAPIByGet 要求 GET 方法

## 安裝 (Install)

- 使用 CLI 安裝 (CLI)
<table><tr><td bgcolor=black>
openupm add com.funtech.soap-internet
</td></tr></table>

- 使用 Git 安裝 (Git)

<table><tr><td bgcolor=black>
https://github.com/cowbear6598/Soap_Internet.git
</td></tr></table>