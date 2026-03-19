using UnityEngine;
using UnityEngine.InputSystem;

namespace OutputEnable.InputSystemCustomizations
{
  /// <summary>
  /// Registers custom composites and interactions with the <see cref="InputSystem"/> at startup.
  /// </summary>
  public static class Registration
  {
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
#endif
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Register()
    {
      InputSystem.RegisterBindingComposite<ButtonPairComposite>();
      InputSystem.RegisterInteraction<ButtonPairInteraction>();
    }
  }
}