using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ACWSSK.App_Code
{
	public enum AppMode 
	{
 		PROD = 0,
		DEMO,
		DEVT
	}

	public enum eReceiptPrinterModel
	{
		StarMicronics,
		Custom,
		Fujitsu
	}

    public enum eModuleStage
    {
        MainIdle = 0,
        InService,
        Payment,
        Offline,
        OutOfService,
        Error,
        Initialize,
        Home, 
        ServiceSelection,
        PaymentSelection,
        PendingCarGoIn,
        Servicing,
        ServicingLogin
    }

    public enum eServicingTask
    {
        Logout,
        Reset,
        Start,
        Stop
    }

    public enum ePaymentStage
    {
        eWalletPayment = 0,
        CardPayment,
        AppPayment,
        ProcessTransaction,
        Success,
        TrxTimeout,
        Failed,
        PerformService,
        ReceiptQR,
        ShowVideo,
        None
    }

    public enum ePaymentStep
    {
        ScanEWallet = 0,
        TapCard,
        App
    }

    public enum eCheckState
    {
        None,
        Disabled,
        Checking,
        CheckPassed,
        Error,
        Warning
    }

    public enum ePaymentMethod
    {
        None = 0,
        eWallet,
        CCards,
        App
    }

    public enum eCarWashMachine
    {
        HEFEI = 0,
        QINGDAO,
        PENTAMASTER
    }

    public enum eQDIotStatus
    {
        READY = 0,
        NOT_READY,
        WASHING,
        OFFLINE,
        CMD_ERROR,
        NOT_ANSWER,
        LOCKING,
        ERROR

    }
    
}
