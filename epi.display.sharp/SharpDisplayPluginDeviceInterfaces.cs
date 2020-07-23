
namespace Epi.Display.Sharp
{

    public interface IHasPower
    {
        void PowerOn();
        void PowerOff();
    }

    public interface IHasPowerToggle
    {
        void PowerToggle();
    }

    public interface IHasInput
    {
        void SelectInput(ushort input);
    }
}