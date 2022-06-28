using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityAgentApi
{
  public static class GaTicket
  {
    public static int TicketBufferLength { get; set; } = 4096;

    public static bool IsInitialized { get { return hInst != 0; } }

    private static int hInst = 0;

    public static bool Initialize(bool reinitialize = false)
    {
      if (IsInitialized)
      {
        if (reinitialize)
          Finalize();
        else
          return true;
      }

      hInst = GaTicketAPI._TcktApi_Initialize();

      return IsInitialized;
    }

    public static uint GetGAVersion(int iComponentType)
    {
      return GaTicketAPI._TcktApi_GetGAVersion(hInst, iComponentType);
    }

    public static int GetTicket(out string ticket)
    {
      uint bufferSize = (uint)TicketBufferLength;
      uint bufferSizeUsed = 0;
      StringBuilder ticketBuffer = new StringBuilder((int)bufferSize);

      int errorCode = GaTicketAPI._TcktApi_getTicket(
        hInst, 
        ticketBuffer, 
        bufferSize, 
        ref bufferSizeUsed);

      //ticket = ticketBuffer.ToString()
      byte[] bytes = Encoding.Unicode.GetBytes(ticketBuffer.ToString());
      ticket = Encoding.UTF8.GetString(bytes);

      return errorCode;
    }

    public static int GetTicketNoAuth(out string ticket)
    {
      uint bufferSize = (uint)TicketBufferLength;
      uint bufferSizeUsed = 0;
      StringBuilder ticketBuffer = new StringBuilder((int)bufferSize);

      int errorCode = GaTicketAPI._TcktApi_getTicketNoAuth(
        hInst, ticketBuffer, bufferSize, ref bufferSizeUsed);

      //ticket = ticketBuffer.ToString();
      byte[] bytes = Encoding.Unicode.GetBytes(ticketBuffer.ToString());
      ticket = Encoding.UTF8.GetString(bytes);
      return errorCode;
    }

    public static int GetNewTicket(out string ticket)
    {
      uint bufferSize = (uint)TicketBufferLength;
      uint bufferSizeUsed = 0;
      StringBuilder ticketBuffer = new StringBuilder((int)bufferSize);

      int errorCode = GaTicketAPI._TcktApi_getNewTicket(
        hInst, ticketBuffer, bufferSize, ref bufferSizeUsed);

      //ticket = ticketBuffer.ToString();
      byte[] bytes = Encoding.Unicode.GetBytes(ticketBuffer.ToString());
      ticket = Encoding.UTF8.GetString(bytes);

      return errorCode;
    }

    public static int DestroyTicket()
    {
      return GaTicketAPI._TcktApi_destroyTicket(hInst);
    }

    public static int GetLastError()
    {
      return GaTicketAPI._TcktApi_getLastError(hInst);
    }

    public static int GetTicketErrorDescription(
      uint dwError, 
      out string errorDescription)
    {
      uint bufferSize = 2048;
      uint bufferSizeUsed = 0;
      StringBuilder descriptionBuffer = new StringBuilder((int)bufferSize);

      int errorCode = GaTicketAPI._TcktApi_getErrorDescription(hInst, dwError, 
        descriptionBuffer, bufferSize, ref bufferSizeUsed);

      errorDescription = descriptionBuffer.ToString();

      return errorCode;
    }

    public static int Finalize()
    {
      int errorCode = GaTicketAPI._TcktApi_Finalize(hInst);
      hInst = 0;
      return errorCode;
    }

    public static bool IsTerminalServiceSession()
    {
      return GaTicketAPI._TcktApi_isTerminalServiceSession(hInst);
    }

    public static uint GetDllVersion()
    {
      return GaTicketAPI.GetDllVersion();
    }

    public static string ReturnCodeAsString(int returnCode)
    {
      string result = $"Not Processed";
      try
      {
        ETicketReturnCodes eTicketCode = (ETicketReturnCodes)returnCode;
        
        result = $"Return Code : {eTicketCode}";
        
      }
      catch (InvalidCastException)
      {
        result = result = $"Unknown Return Code {returnCode}";
      }
      catch (Exception ex)
      {
        result = $"Error processing eTicketError return code {returnCode}." +
          $" Error Message : {ex.Message}";
      }

      return result;
      
    }

  }
}
