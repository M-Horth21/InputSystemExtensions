using System.ComponentModel;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;

namespace OutputEnable.InputSystemCustomizations
{
  /// <summary>
  /// A binding composite that treats two buttons as a single input.
  /// </summary>
  /// <remarks>
  /// Outputs three distinct values:
  /// <list type="table">
  /// <item><term>0</term><description> neither button is pressed</description></item>
  /// <item><term><see cref="PARTIAL_PRESS"/></term><description> exactly one button is pressed.
  /// This value is intentionally below Unity's default press threshold (0.5)
  /// so that holding a single button does not actuate the action.</description></item>
  /// <item><term>1</term><description> both buttons are pressed</description></item>
  /// </list>
  /// Intended for use with <see cref="ButtonPairInteraction"/>, which monitors
  /// the transition from 0 to 1 within a configurable time window.
  /// </remarks>
  [DisplayStringFormat("{one}+{two}")]
  [DisplayName("Button Pair")]
  public class ButtonPairComposite : InputBindingComposite<float>
  {
    /// <summary>
    /// Value returned when exactly one of the two buttons is pressed.
    /// Intentionally set below Unity's press threshold (default 0.5) 
    /// in case this binding is used with standard interactions.
    /// </summary>
    const float PARTIAL_PRESS = 0.1f;

    /// <summary>The first button in the pair.</summary>
    [InputControl(layout = "Button")]
    public int ButtonA;

    /// <summary>The second button in the pair.</summary>
    [InputControl(layout = "Button")]
    public int ButtonB;

    /// <inheritdoc/>
    public override float ReadValue(ref InputBindingCompositeContext context)
    {
      var firstButton = context.ReadValueAsButton(ButtonA);
      var secondButton = context.ReadValueAsButton(ButtonB);

      if (firstButton ^ secondButton) return PARTIAL_PRESS;
      if (firstButton && secondButton) return 1;
      return 0;
    }

    /// <inheritdoc/>
    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
      => ReadValue(ref context);
  }
}