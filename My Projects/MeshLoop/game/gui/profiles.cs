//---------------------------------------------------------------------------------------------
// GuiDefaultProfile is a special profile that all other profiles inherit defaults from. It
// must exist.
//---------------------------------------------------------------------------------------------
singleton GuiControlProfile (GuiDefaultProfile)
{
   tab = false;
   canKeyFocus = false;
   hasBitmapArray = false;
   mouseOverSelected = false;

   // fill color
   opaque = false;
   fillColor = "242 241 240";
   fillColorHL ="228 228 235";
   fillColorSEL = "98 100 137";
   fillColorNA = "255 255 255 ";

   // border color
   border = 0;
   borderColor   = "100 100 100"; 
   borderColorHL = "50 50 50 50";
   borderColorNA = "75 75 75"; 

   // font
   fontType = "Arial";
   fontSize = 14;
   fontCharset = ANSI;

   fontColor = "0 0 0";
   fontColorHL = "0 0 0";
   fontColorNA = "0 0 0";
   fontColorSEL= "255 255 255";

   // bitmap information
   bitmap = "";
   bitmapBase = "";
   textOffset = "0 0";

   // used by guiTextControl
   modal = true;
   justify = "left";
   autoSizeWidth = false;
   autoSizeHeight = false;
   returnTab = false;
   numbersOnly = false;
   cursorColor = "0 0 0 255";

   // sounds
   //soundButtonDown = "";
   //soundButtonOver = "";
};

singleton GuiControlProfile (GuiToolTipProfile)
{
   // fill color
   fillColor = "239 237 222";

   // border color
   borderColor   = "138 134 122";

   // font
   fontType = "Arial";
   fontSize = 14;
   fontColor = "0 0 0";

};

singleton GuiControlProfile (GuiContentProfile)
{
   opaque = true;
   fillColor = "255 255 255";
};

singleton GuiControlProfile( GuiButtonProfile )
{
   opaque = true;
   border = true;
	 
   fontColor = "50 50 50";
   fontColorHL = "0 0 0";
	 fontColorNA = "200 200 200";
	 //fontColorSEL ="0 0 0";
   fixedExtent = false;
   justify = "center";
   canKeyFocus = false;
	bitmap = "gui/images/button";
   hasBitmapArray = false;
};

singleton GuiControlProfile( GuiLeftDropdownButtonProfile : GuiButtonProfile )
{
   justify = "left";
};

singleton GuiControlProfile (GuiWindowProfile)
{
   opaque = false;
   border = 2;
   fillColor = "242 241 240";
   fillColorHL = "221 221 221";
   fillColorNA = "200 200 200";
   fontColor = "50 50 50";
   fontColorHL = "0 0 0";
   bevelColorHL = "255 255 255";
   bevelColorLL = "0 0 0";
   text = "untitled";
   bitmap = "gui/images/window";
   textOffset = "8 4";
   hasBitmapArray = true;
   justify = "left";
   yPositionOffset = "21";
};

singleton GuiControlProfile (GuiToolbarWindowProfile : GuiWindowProfile)
{
   bitmap = "gui/images/toolbar-window";
   text = "";
};  

singleton GuiControlProfile (GuiMenubarWindowProfile : GuiWindowProfile)
{
   opaque = true;
   fillColor = "242 241 240";
   bitmap = "gui/images/menubar-window";
   text = "";
}; 

singleton GuiControlProfile( GuiTabBookProfile )
{
   fillColorHL = "100 100 100";
   fillColorNA = "150 150 150";
   fontColor = "30 30 30";
   fontColorHL = "0 0 0";
   fontColorNA = "50 50 50";
   fontType = "Arial";
   fontSize = 14;
   justify = "center";
   bitmap = "gui/images/tab";
   tabWidth = 64;
   tabHeight = 24;
   tabPosition = "Top";
   tabRotation = "Horizontal";
   textOffset = "0 -3";
   tab = true;
   cankeyfocus = true;
   //border = false;
   //opaque = false;
};

singleton GuiControlProfile( GuiTabPageProfile : GuiDefaultProfile )
{
		fontType = "Arial";
   fontSize = 10;
   justify = "center";
   bitmap = "gui/images/tab";
   opaque = false;
   fillColor = "240 239 238";
};

singleton GuiControlProfile( GuiScrollProfile )
{
   opaque = true;
   fillcolor = "255 255 255";
   fontColor = "0 0 0";
   fontColorHL = "150 150 150";
   //borderColor = GuiDefaultProfile.borderColor;
   border = true;
   bitmap = "gui/images/scrollBar";
   hasBitmapArray = true;
};

singleton GuiControlProfile( GuiCheckBoxProfile )
{
   opaque = false;
   fillColor = "232 232 232";
   border = false;
   borderColor = "100 100 100";
   fontSize = 14;
   fontColor = "20 20 20";
   fontColorHL = "80 80 80";
	fontColorNA = "200 200 200";
   fixedExtent = true;
   justify = "left";
   bitmap = "gui/images/checkbox";
   hasBitmapArray = true;
};

singleton GuiControlProfile( GuiCheckBoxListProfile : GuiCheckBoxProfile)
{
   bitmap = "gui/images/checkbox-list";
};

singleton GuiControlProfile( GuiRadioProfile )
{
   fontSize = 14;
   fillColor = "232 232 232";
	/*fontColor = "200 200 200";
   fontColorHL = "255 255 255";*/
   fontColor = "20 20 20";
   fontColorHL = "80 80 80";
   fixedExtent = true;
   bitmap = "gui/images/radioButton";
   hasBitmapArray = true;
};

singleton GuiControlProfile (GuiTextProfile)
{
   justify = "left";
   fontColor = "20 20 20";
};

singleton GuiControlProfile (GuiCenterTextProfile : GuiTextProfile)
{
   justify = "center";
};

singleton GuiControlProfile (GuiCenterLightTextProfile : GuiCenterTextProfile)
{
   fontColor = "200 200 200";
};

singleton GuiControlProfile (GuiCenterLightLargeTextProfile : GuiCenterLightTextProfile)
{
   fontSize = 24;
};

singleton GuiControlProfile (GuiCenterMidLargeTextProfile : GuiCenterLightLargeTextProfile)
{
   fontColor = "140 140 140";
};

singleton GuiControlProfile (GuiLightLargeTextProfile : GuiTextProfile)
{
   fontSize = 24;
   fontColor = "200 200 200";
};

singleton GuiControlProfile (GuiRightLightLargeTextProfile : GuiLightLargeTextProfile)
{
   justify = "right";
};

singleton GuiControlProfile(GuiTextListProfile : GuiTextProfile)
{
   tab = true;
   canKeyFocus = true;
   fontColorNA = "75 75 75";
};

singleton GuiControlProfile( GuiSliderProfile )
{
   bitmap = "gui/images/slider";
};


singleton GuiControlProfile( GuiPopUpMenuDefault : GuiDefaultProfile )
{
   opaque = true;
   mouseOverSelected = true;
   textOffset = "3 3";
   border = 0;
   borderThickness = 0;
   fixedExtent = true;
   bitmap = "gui/images/scrollbar";
   hasBitmapArray = true;
   profileForChildren = GuiPopupMenuItemBorder;
   fillColor = "242 241 240 ";//"255 255 255";//100
   fillColorHL = "228 228 235 ";//"204 203 202";
   fillColorSEL = "98 100 137 ";//"204 203 202";
   // font color is black
   fontColorHL = "0 0 0 ";//"0 0 0";
   fontColorSEL = "255 255 255";//"0 0 0";
   borderColor = "100 100 100";
};

singleton GuiControlProfile( GuiPopupBackgroundProfile )
{
   modal = true;
};

singleton GuiControlProfile( GuiPopUpMenuProfile : GuiPopUpMenuDefault )
{
   textOffset         = "6 4";
   bitmap             = "gui/images/dropDown";
   hasBitmapArray     = true;
   border             = 1;
   profileForChildren = GuiPopUpMenuDefault;
};

singleton GuiControlProfile( GuiPopUpMenuTabProfile : GuiPopUpMenuDefault )
{
   bitmap             = "gui/images/dropDown-tab";
   textOffset         = "6 4";
   canKeyFocus        = true;
   hasBitmapArray     = true;
   border             = 1;
   profileForChildren = GuiPopUpMenuDefault;
};

singleton GuiControlProfile( GuiPopUpMenuEditProfile : GuiPopUpMenuDefault )
{
   textOffset         = "6 4";
   canKeyFocus        = true;
   bitmap             = "gui/images/dropDown";
   hasBitmapArray     = true;
   border             = 1;
   profileForChildren = GuiPopUpMenuDefault;
};

// -----------------------------------------------------------

singleton GuiControlProfile( GuiCamRadioProfile : GuiRadioProfile )
{
   bitmap = "gui/images/lighting/cam";
};

singleton GuiControlProfile( GuiLight1RadioProfile : GuiRadioProfile )
{
   bitmap = "gui/images/lighting/light1";
};

singleton GuiControlProfile( GuiLight2RadioProfile : GuiRadioProfile )
{
   bitmap = "gui/images/lighting/light2";
};

singleton GuiControlProfile( GuiLight3RadioProfile : GuiRadioProfile )
{
   bitmap = "gui/images/lighting/light3";
};

singleton GuiControlProfile( GuiGroupBorderProfile )
{
   border = false;
   opaque = false;
   hasBitmapArray = true;
   bitmap = "gui/images/group-border";
};

singleton GuiControlProfile( GuiInspectorSwatchButtonProfile )
{
   borderColor = "100 100 100 255";
   borderColorNA = "200 200 200 255";
   fillColorNA = "255 255 255 0";
   borderColorHL = "0 0 0 255";
};

singleton GuiControlProfile( GuiTextEditProfile )
{
   opaque = true;
   bitmap = "gui/images/textEdit";
   hasBitmapArray = true; 
   border = 0; // fix to display textEdit img
   //borderWidth = "1";  // fix to display textEdit img
   //borderColor = "100 100 100";
   fillColor = "242 241 240 0";
   fillColorHL = "255 255 255 0";
   fontColor = "140 140 140";
   fontColorHL = "255 255 255";
   fontColorSEL = "98 100 137";
   fontColorNA = "200 200 200";
   textOffset = "4 0";
   fontSize = 24;
   autoSizeWidth = false;
   autoSizeHeight = true;
   justify = "center";
   tab = false;
   canKeyFocus = true;
   cursorColor = "255 255 255";
};

singleton GuiControlProfile( GuiTextEditProfileNumbersOnly : GuiTextEditProfile )
{
   numbersOnly = true;
};

singleton GuiControlProfile( GuiLightTextEditProfile : GuiTextEditProfile )
{
   fontColor = "200 200 200";
};

singleton GuiControlProfile( GuiLightLeftTextEditProfile : GuiTextEditProfile )
{
   fontColor = "200 200 200";
   justify = "left";
};

singleton GuiControlProfile( GuiConsoleProfile )
{
   fontType = ($platform $= "macos") ? "Monaco" : "Lucida Console";
   fontSize = ($platform $= "macos") ? 13 : 12;
    fontColor = "255 255 255";
    fontColorHL = "0 255 255";
    fontColorNA = "255 0 0";
    fontColors[6] = "100 100 100";
    fontColors[7] = "100 100 0";
    fontColors[8] = "0 0 100";
    fontColors[9] = "0 100 0";
};

singleton GuiControlProfile( GuiConsoleTextEditProfile : GuiTextEditProfile )
{
   fontType = ($platform $= "macos") ? "Monaco" : "Lucida Console";
   fontSize = ($platform $= "macos") ? 13 : 12;
   justify = "left";
   fontColor = "0 0 0 255";
};

singleton GuiControlProfile( GuiConsoleTextProfile )
{   
   fontColor = "0 0 0";
   autoSizeWidth = true;
   autoSizeHeight = true;   

   textOffset = "2 2";
   opaque = true;   
   fillColor = "255 255 255";
   border = true;
   borderThickness = 1;
   borderColor = "0 0 0";
};

$ConsoleDefaultFillColor = "0 0 0 175";

singleton GuiControlProfile( ConsoleScrollProfile : GuiScrollProfile )
{
	opaque = true;
	fillColor = $ConsoleDefaultFillColor;
	border = 1;
	//borderThickness = 0;
	borderColor = "0 0 0";
};

singleton GuiControlProfile( ConsoleTextEditProfile : GuiTextEditProfile )
{
   fillColor = "200 200 200 255";
   fillColorHL = "255 255 255 255";   
   fontColor = "0 0 0 255";
   fontType = ($platform $= "macos") ? "Monaco" : "Lucida Console";
   fontSize = ($platform $= "macos") ? 13 : 12;
   justify = "left";
};

singleton GizmoProfile( GlobalGizmoProfile )
{
   screenLength = 100;
};

// warning material
singleton Material( WarningMaterial )
{
   diffuseMap[0] = "gui/images/warnMat";
   emissive[0] = true;
};

