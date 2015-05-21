
if ( isObject( moveMap ) )
   moveMap.delete();
new ActionMap(moveMap);

exec("./profiles.cs");
exec("./hud.cs");
exec("./cam.cs");
exec("./statusbar.cs");
exec("./nodes.cs");
exec("./filedialog.cs");
exec("./meshes.cs");
exec("./skins.cs");
exec("./sequences.cs");
exec("./lights.cs");
exec("./bg.cs");
exec("./menu.cs");

GlobalActionMap.bind(keyboard, "ctrl minus", toggleConsole);
GlobalActionMap.bind(keyboard, up,    playPrevSequence);
GlobalActionMap.bind(keyboard, down,  playNextSequence);
GlobalActionMap.bind(keyboard, space, playPause);
GlobalActionMap.bind(keyboard, left,  gotoPrevFrame);
GlobalActionMap.bind(keyboard, right, gotoNextFrame);

