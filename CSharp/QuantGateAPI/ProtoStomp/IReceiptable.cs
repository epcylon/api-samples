namespace BridgeRock.CSharpExample.ProtoStomp
{
    public interface IReceiptable
    {
        ulong ReceiptID { get; }

        void OnReceipt();

        void OnInvalidate();
    }
}
