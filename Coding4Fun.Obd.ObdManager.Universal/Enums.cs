namespace Coding4Fun.Obd.ObdManager
{
	public enum Protocol
	{
		Unknown = -1,
		ElmAutomatic = 0,
		SaeJ1850Pwm,
		SaeJ1850Vpw,
		Iso91412,
		Iso142304Kwp5Baud104Kbaud,
		Iso142304KwpFast104Kbaud,
		Iso157654Can11Bit500Kbaud,
		Iso157654Can29Bit500Kbaud,
		Iso157654Can11Bit250Kbaud,
		Iso157654Can29Bit250Kbaud,
	}
}
