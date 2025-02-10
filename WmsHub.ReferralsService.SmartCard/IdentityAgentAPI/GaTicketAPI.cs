using System.Runtime.InteropServices;
using System.Text;

namespace IdentityAgentApi
{
  public static class GaTicketAPI
  {
    [DllImport("TicketApiDll.dll")]
    public static extern int _TcktApi_Initialize();

    [DllImport("TicketApiDll.dll")]
    public static extern uint _TcktApi_GetGAVersion(
      int ticketApiInstance, 
      int iComponentType);

    [DllImport("TicketApiDll.dll", CharSet = CharSet.Unicode)]
    public static extern int _TcktApi_getTicket(
      int ticketApiInstance,
      StringBuilder pszTicketBuffer,
      uint iTicketBufferSize,
      ref uint piUsedTicketSize);

    [DllImport("TicketApiDll.dll", CharSet = CharSet.Unicode)]
    public static extern int _TcktApi_getTicketNoAuth(
      int ticketApiInstance,
      StringBuilder pszTicketBuffer,
      uint iTicketBufferSize,
      ref uint piUsedTicketSize);

    [DllImport("TicketApiDll.dll", CharSet = CharSet.Unicode)]
    public static extern int _TcktApi_getNewTicket(
      int ticketApiInstance,
      StringBuilder pszTicketBuffer,
      uint iTicketBufferSize,
      ref uint piUsedTicketSize);

    [DllImport("TicketApiDll.dll")]
    public static extern int _TcktApi_destroyTicket(
      int ticketApiInstance);

    [DllImport("TicketApiDll.dll")]
    public static extern int _TcktApi_getLastError(
      int ticketApiInstance);

    [DllImport("TicketApiDll.dll", CharSet = CharSet.Unicode)]
    public static extern int _TcktApi_getErrorDescription(
      int ticketApiInstance,
      uint dwError,
      StringBuilder pszDescrBuffer,
      uint iDescrBufferSize,
      ref uint piUsedDescrSize);

    [DllImport("TicketApiDll.dll")]
    public static extern int _TcktApi_Finalize(
      int ticketApiInstance);

    [DllImport("TicketApiDll.dll")]
    public static extern bool _TcktApi_isTerminalServiceSession(
      int ticketApiInstance);

    [DllImport("TicketApiDll.dll")]
    public static extern uint GetDllVersion();
  }

  public enum ETicketReturnCodes : uint
  {
    // Error Codes
    TCK_API_SUCCESS = 0x00000000,

    // Error groups
    TCK_ERR_GRP_MASK = 0x80000000,
    TCK_ERR_GRP_GENERIC = 0x80100000,
    TCK_ERR_GRP_CARD_DEVICE = 0x80200000,
    TCK_ERR_GRP_AUTHENTICATION = 0x80400000,
    TCK_ERR_GRP_CONFIG = 0x80800000,
    TCK_ERR_GRP_WINDOWS = 0x81000000,

    //GENERIC ERRORS
    TCK_API_ERR_INTERNAL =
      0x00000001 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_NOT_INITIALIZED = 
      0x00000002 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_INVALID_PARAMETER = 
      0x00000003 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_BUFFER_TOO_SMALL = 
      0x00000004 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_INVALID_INSTANCE_HANDLE = 
      0x00000005 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_SERVER_BUSY = 
      0x00000006 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_SERVER_COMM_FAILED = 
      0x00000007 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_RESOURCE_ALLOC_FAILED = 
      0x00000008 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_TICKET_NOT_AVAILABLE = 
      0x00000009 | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_INTERNAL_ERROR = 
      0x0000000A | TCK_ERR_GRP_GENERIC,
    TCK_API_ERR_NOT_IMPLEMENTED = 
      0x0000000b | TCK_ERR_GRP_GENERIC,

    //CONFIGURATION ERRORS
    TCK_API_ERR_VERSION_CONFLICT = 
      0x00000001 | TCK_ERR_GRP_CONFIG,
    TCK_API_ERR_NOT_INSTALLED = 
      0x00000002 | TCK_ERR_GRP_CONFIG,
    TCK_API_ERR_NO_SERVERFOUND = 
      0x00000003 | TCK_ERR_GRP_CONFIG,
    TCK_API_ERR_LOAD_RES_FAILED = 
      0x00000004 | TCK_ERR_GRP_CONFIG,
    TCK_API_ERR_AUTHMETHOD_NOT_AVAILABLE = 
      0x00000005 | TCK_ERR_GRP_CONFIG,
    TCK_API_ERR_AUTHMETHOD_NOT_POSSIBLE = 
      0x00000006 | TCK_ERR_GRP_CONFIG,
    TCK_API_ERR_RESOURCE_NOT_AVAILABLE = 
      0x00000007 | TCK_ERR_GRP_CONFIG,

    //CARD or DEVICE RELATED ERRORS
    TCK_API_ERR_SMARTCARD_NOT_SUPPORTED = 
      0x00000001 | TCK_ERR_GRP_CARD_DEVICE,
    TCK_API_ERR_READER_NOT_AVAILABLE = 
      0x00000002 | TCK_ERR_GRP_CARD_DEVICE,
    TCK_API_ERR_SMARTCARD_NOT_VALID = 
      0x00000003 | TCK_ERR_GRP_CARD_DEVICE,
    TCK_API_ERR_PIN_BLOCKED = 
      0x00000004 | TCK_ERR_GRP_CARD_DEVICE,
    TCK_API_ERR_CARD_TRANSACTION_FAILED = 
      0x00000005 | TCK_ERR_GRP_CARD_DEVICE,
    TCK_API_ERR_NO_CERT_FOUND = 
      0x00000006 | TCK_ERR_GRP_CARD_DEVICE,
    TCK_API_ERR_NO_KEY_FOUND = 
      0x00000007 | TCK_ERR_GRP_CARD_DEVICE,

    //AUTHENTICATION ERRORS
    TCK_API_ERR_USER_ABORT = 
      0x00000001 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_AUTHENTICATION_FAILED = 
      0x00000002 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_AUTHENTICATION_NOT_POSSIBLE = 
      0x00000003 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_USER_NOT_VALID = 
      0x00000004 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_AUTHSERVER_NOT_AVAILABLE = 
      0x00000005 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_AUTHDATA_SYNTAX_ERROR = 
      0x00000006 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_AUTH_NOT_VALID = 
      0x00000007 | TCK_ERR_GRP_AUTHENTICATION,
    TCK_API_ERR_AUTHMETHOD_NOT_SUPPORTED = 
      0x00000008 | TCK_ERR_GRP_AUTHENTICATION
  }
}
