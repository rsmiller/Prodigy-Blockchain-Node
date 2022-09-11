using System;
using System.Collections.Generic;
using System.Text;

namespace Prodigy.BusinessLayer
{

	public enum BlockchainEvent
    {
		Added = 1
    }


	public enum CommandType
	{
		AddServerNode = 1,
		RequestCryptoBlockchain = 2,
		ReceiveCryptoBlockchain = 3,
		ValidateBlock = 4,
		BlockAccepted = 5,
		BlockDenied = 6,
        RequestMachineInfo = 6
    }

	public enum MiningEventCompleteType
	{
		Failed,
		Success
	}

	public enum NetworkType
	{
		Mainnet,
        Testnet
    }

    public enum LogLevel
    { 
		Info = 3,
		Warning = 2,
		Error = 1
	}

	public enum LogType
	{
		Console = 1,
		MySql = 2,
		MSSQL = 3
	}
}
