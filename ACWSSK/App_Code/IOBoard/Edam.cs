using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;

namespace IOBoard
{

	public class Edam : SerialPortHandler
	{
		#region Field

		private string traceCategory = "SerialPort";
		private List<byte> bResponse = new List<byte>();
		private AutoResetEvent autoResetEvent = new AutoResetEvent(false);
		private byte CR = 0x0D;
		public static int[] BaudRate = new int[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 };

		protected Dictionary<string, int> dictBaudRateCodes = new Dictionary<string, int> {
            {"03", 1200},
            {"04", 2400},
            {"05", 4800},
            {"06", 9600},
            {"07", 19200},
            {"08", 38400},
            {"09", 57600},
            {"0A", 115200}
        };


		#endregion


		#region Variable & Enum

		public enum eDigitalOutput
		{
			DO0 = 0,
			DO1,
			DO2,
			DO3,
			DO4,
			DO5,
			DO6,
			DO7
		}

		public enum eDigitalInput
		{
			DI0 = 0,
			DI1,
			DI2,
			DI3,
			DI4,
			DI5,
			DI6
		}

		#endregion

		#region Contructor

		public Edam()
		{

		}

		#endregion

		#region General Function

		public bool CheckSum(string command, out byte bCheckSum)
		{
			bool success = false;
			bCheckSum = 0;
			try
			{
				if (string.IsNullOrEmpty(command))
					throw new Exception("Command Empty");

				byte[] bCommand = StringToHex(command);

				// calc checksum 
				// cummulative all byte
				foreach (byte b in bCommand)
				{
					bCheckSum += b;
				}

				success = true;

				
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] CheckSum = {0}", ex.ToString()), traceCategory);
			}

			return success;
		}

		public bool Encode(string command, bool checkSumEnabled, out byte[] bCommand)
		{
			bool success = false;
			bCommand = null;
			try
			{

				if (string.IsNullOrEmpty(command))
					throw new Exception("Command Empty");

				byte bCheckSum = 0;


				bCommand = StringToHex(command);

				if (checkSumEnabled)
				{
					if (!CheckSum(command, out bCheckSum))
						throw new Exception("Unable to calculate CheckSum");
					bCommand = bCommand.Concat(new byte[] { bCheckSum }).ToArray();
				}

				bCommand = bCommand.Concat(new byte[] { CR }).ToArray();


				

				success = true;
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] Encode = {0}", ex.ToString()), traceCategory);
			}
			return success;
		}

		public byte[] StringToHex(string input)
		{
			byte[] output = null;
			try
			{
				output = Encoding.ASCII.GetBytes(input);

			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] StringToHex = {0}", ex.ToString()), traceCategory);
			}
			

			return output;
		}

		private byte[] Decode(byte[] bResp)
		{
			//? is a delimiter character which indicates an invalid command.

			if (!(bResp.Count() > 0 && bResp[bResp.Length - 1] == CR))
				throw new Exception("No Response!");

			if (bResp[0] == Encoding.ASCII.GetBytes("?")[0])
				throw new Exception("Invalid Command!");

			return bResp;
		}

		#endregion

		#region Serial Function

		protected bool SendCommand(byte[] cmd, string validDelimiter, out string hData)
		{
			hData = null;
			try
			{
				bResponse.Clear();

				
				comPort.Write(cmd, 0, cmd.Length);

				autoResetEvent.Reset();
				autoResetEvent.WaitOne(200);

				if (validDelimiter != null)
				{
					hData = Encoding.ASCII.GetString(Decode(bResponse.ToArray()));

					if (hData.Substring(0, 1) != validDelimiter)
						throw new Exception("Delimiter is not Matched!");
				}

				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		protected override void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				int iCount = comPort.BytesToRead;
				byte[] bBuffer = new byte[iCount];

				comPort.Read(bBuffer, 0, iCount);

				bResponse.AddRange(bBuffer);

				if (bResponse.Last() == CR)
					autoResetEvent.Set();

			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] comPort_DataReceived: {0}", ex.ToString()), traceCategory);
			}

		}

		#endregion

		#region Edam Function

		// pending
		public void SetModuleConfiguration(byte bOldModule, byte bNewModule, string baudRate)
		{

			string oldModule = BitConverter.ToString(new byte[] { bOldModule });
			string newModule = BitConverter.ToString(new byte[] { bNewModule });
			string formatData = "00";

			string command = string.Format("${0}{1}{2}{3}", oldModule, newModule, baudRate, formatData);
			byte[] bcommand = null;
			if (Encode(command, false, out bcommand))
			{

			}

		}

		public bool ReadModuleConfiguration(byte bModule, out int moduleAddress, out int baudRate)
		{
			bool success = false;
			moduleAddress = 0;
			baudRate = 0;
			try
			{
				string Module = BitConverter.ToString(new byte[] { bModule });
				string command = string.Format("${0}2", Module);

				string hData;
				byte[] bCommand = null;
				if (Encode(command, false, out bCommand))
				{
					if (SendCommand(bCommand, "!", out hData))
					{
						moduleAddress = Convert.ToInt16(hData.Substring(1, 2));
						baudRate = dictBaudRateCodes[hData.Substring(5, 2)];
						success = true;
					}

				}
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] ReadModuleConfiguration = {0}", ex.ToString()), traceCategory);
			}
			return success;
		}

		public bool DigitalDataIn(byte moduleAddress, out Dictionary<eDigitalOutput, bool> digitalOutput, out Dictionary<eDigitalInput, bool> digitalInput)
		{
			bool success = false;
			digitalOutput = new Dictionary<eDigitalOutput, bool>();
			digitalInput = new Dictionary<eDigitalInput, bool>();
			try
			{
				//$AA6(cr)
				//!(digitalOutput)(digitalInput)00(cr)
				string hData;
				string Module = BitConverter.ToString(new byte[] { moduleAddress });
				string command = string.Format("${0}6", Module);

				byte[] bCommand = null;
				if (Encode(command, false, out bCommand))
				{
					if (SendCommand(bCommand, "!", out hData))
					{
						ByteToDigitalInput(ref digitalInput, ConvertHexToByteArray(hData.Substring(3, 2))[0]);
						ByteToDigitalOutput(ref digitalOutput, ConvertHexToByteArray(hData.Substring(1, 2))[0]);

						success = true;
					}
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] DigitalDataIn = {0}", ex.ToString()), traceCategory);
			}
			return success;
		}

		public bool DigitalDataOut(byte moduleAddress, Dictionary<eDigitalOutput, bool> digitalOutput)
		{
			bool success = false;
			try
			{
				//$AA6(cr)
				//!(digitalOutput)(digitalInput)00(cr)
				string hData;
				string Module = BitConverter.ToString(new byte[] { moduleAddress });
				string output = BitConverter.ToString(new byte[] { DigitalOutputToByte(digitalOutput) });
				string command = string.Format("#{0}00{1}", Module, output);

				byte[] bCommand = null;
				if (Encode(command, false, out bCommand))
				{
					if (SendCommand(bCommand, ">", out hData))
						success = true;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] DigitalDataIn = {0}", ex.ToString()), traceCategory);
			}
			return success;
		}

		public bool DigitalDataOut(byte moduleAddress, eDigitalOutput digitalOutput, bool value)
		{
			bool success = false;
			try
			{
				//$AA6(cr)
				//!(digitalOutput)(digitalInput)00(cr)
				string hData;
				string Module = BitConverter.ToString(new byte[] { moduleAddress });

				string command = string.Format("#{0}1{1}{2}", Module, (int)digitalOutput, value ? "01" : "00");

				byte[] bCommand = null;
				if (Encode(command, false, out bCommand))
				{
					if (SendCommand(bCommand, ">", out hData))
						success = true;
				}
			}
			catch (Exception ex)
			{
				Trace.WriteLineIf(swcTraceLevel.TraceError, string.Format("[Error] DigitalDataIn = {0}", ex.ToString()), traceCategory);
			}
			return success;
		}

		#endregion

		#region Helper

		private byte[] ConvertHexToByteArray(string hex)
		{
			hex = hex.Replace(" ", "");

			byte[] comBuffer = new byte[hex.Length / 2];

			for (int i = 0; i < hex.Length; i += 2)
				comBuffer[i / 2] = (byte)Convert.ToByte(hex.Substring(i, 2), 16);

			return comBuffer;
		}

		private void ByteToDigitalOutput(ref Dictionary<eDigitalOutput, bool> collection, byte value)
		{
			for (int i = 0; i < 8; i++)
				collection.Add((eDigitalOutput)i, ((int)value & (int)Math.Pow(2, (i + 1) - 1)) > 0);
		}

		private void ByteToDigitalInput(ref Dictionary<eDigitalInput, bool> collection, byte value)
		{
			for (int i = 0; i < 4; i++)
				collection.Add((eDigitalInput)i, ((int)value & (int)Math.Pow(2, (i + 1) - 1)) > 0);
		}

		private byte DigitalOutputToByte(Dictionary<eDigitalOutput, bool> collection)
		{
			int value = 0;

			foreach (KeyValuePair<eDigitalOutput, bool> row in collection)
				value |= row.Value ? (int)Math.Pow(2, ((int)row.Key + 1) - 1) : 0;

			return (byte)value;
		}

		#endregion

	}
}
