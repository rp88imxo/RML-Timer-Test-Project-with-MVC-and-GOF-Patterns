using Cysharp.Threading.Tasks;

namespace RML.Core
{
public interface IScreen
{
    ScreenType ScreenType { get; }

    void RegisterScreen();

    void UnregisterScreen();

    UniTask OnShow<T>(T baseScreenData) where T : BaseScreenData;

    UniTask OnHide();
}
}