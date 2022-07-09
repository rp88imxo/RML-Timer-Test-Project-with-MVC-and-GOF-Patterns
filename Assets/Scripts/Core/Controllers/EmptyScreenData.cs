
namespace RML.Core
{
public class EmptyScreenData : BaseScreenData
{
    public sealed override ScreenType ScreenType { get; set; }

    public EmptyScreenData(ScreenType screenType)
    {
        ScreenType = screenType;
    }
}

}