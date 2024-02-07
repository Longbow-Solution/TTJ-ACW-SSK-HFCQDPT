
洗车模式1， x30
洗车模式2， x31
洗车模式3， x32

启动 x33
暂停 x34
取消 x35

您的付款设备 收到钱后，给我们机器 x33启动信号就可以. 24dc

Get Status IOT
https://github.com/cheyoudaren/global-open-server-api-interface-description/wiki



机器每次洗完车， 机器会自动处于standby状态，随时 可以开始 下一次洗车。

机器处于standby状态时， 机器的PLC  y23 会输出24dc.


如果机器没有处在standby状态，您的付款设备可以 给机器 PLC x36 一个 24dc 信号，  机器就可以处在 standby状态。

x36相当于 reset功能。

https://global-open-cn-test.cheyoudaren.com/
openId rWeFxGEAPeuVMfwE
secret vdLzHAQJlcayVmLL
iotId 4