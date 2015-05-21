
function setCam(%viewid) {
   View.changeView(false);
   View.setView(%viewid);
   // verify that this was accepted
   %viewid = View.getView();
   switch (%viewid) {
      case 1:
         GuiLight1Btn.setStateOn(true);
         lightSetupVisible(1);
      case 2:
         GuiLight2Btn.setStateOn(true);
         lightSetupVisible(2);
      case 3:
         GuiLight3Btn.setStateOn(true);
         lightSetupVisible(3);
      default:
         GuiCamBtn.setStateOn(true);
         lightSetupVisible(0);
   }
}

function lightSetupVisible(%light)
{
   $currView = %light;
   if (%light>0) {
      %light = %light - 1;
      LightSetupWindow.visible = true;
      LightSetupWindow.text = "Light " @ (%light+1) @ " settings";

      eval("%color = $Pref::MeshLoop::light" @ %light @ "::color;");
      %color = setWord(%color, 3, "1");
      LightSetupColor.color = %color;
      LightSetupColorCode.text = "#" @ ColorFToHex(%color) @ " - " @  ColorFloatToInt(%color, true);
      LightSetupColor.Command = "getColorF($Pref::MeshLoop::light" @ %light @ "::color, \"LightSetupColor.updateColor\");";

      eval("%ambient = $Pref::MeshLoop::light" @ %light @ "::ambient;");
      %ambient = setWord(%ambient, 3, "1");
      LightSetupAmbient.color = %ambient;
      LightSetupAmbientCode.text = "#" @ ColorFToHex(%ambient) @ " - " @  ColorFloatToInt(%ambient, true);
      LightSetupAmbient.Command = "getColorF($Pref::MeshLoop::light" @ %light @ "::ambient, \"LightSetupAmbient.updateColor\");";
      
      eval("%brightness = $Pref::MeshLoop::light" @ %light @ "::brightness;");
      BrightnessSlider.setValue(%brightness);

      eval("%state = $Pref::MeshLoop::light" @ %light @ "::enabled;");
      LightEnabled.setStateOn(%state);
   } else {
      LightSetupWindow.visible = false;
   }
}

function LightEnabled::onClick(%this)
{
   eval("$Pref::MeshLoop::light" @ $currView-1 @ "::enabled = %this.getStateOn();");
}

function onBrightnessChanged(%newbrightness)
{
   BrightnessSlider.setValue(%newbrightness);
}

function BrightnessSlider::onValueChanged(%this)
{
   eval("$Pref::MeshLoop::light" @ ($currView-1) @ "::brightness = %this.value;");
   BrightnessSliderVal.text = %this.getValue();
}

function LightSetupColor::updateColor(%this, %color)
{
   %color = setWord(%color, 3, "1");
   eval("$Pref::MeshLoop::light" @ ($currView-1) @ "::color = \"" @ %color @ "\";");
   LightSetupColor.color = %color;
   LightSetupColorCode.text = "#" @ ColorFToHex(%color) @ " - " @ ColorFloatToInt(%color, true);
   return false;
}

function LightSetupAmbient::updateColor(%this, %color)
{
   %color = setWord(%color, 3, "1");
   eval("$Pref::MeshLoop::light" @ ($currView-1) @ "::ambient = \"" @ %color @ "\";");
   LightSetupAmbient.color = %color;
   LightSetupAmbientCode.text = "#" @ ColorFToHex(%color) @ " - " @ ColorFloatToInt(%color, true);
   return false;
}

function onLightClick(%light) {
   eval("$Pref::MeshLoop::light" @ %light @ "::enabled = !$Pref::MeshLoop::light" @ %light @ "::enabled;");
}
