// See https://aka.ms/new-console-template for more information
using EngineProject.Helpers;
using EngineProject.Enums;
using System.Windows.Forms;
using System.Drawing;
using EngineProject.Structures;
using EngineProject.Managers;
using EngineProject.Infrastructure;

/*
Point testPos1 = new Point();
Point testPos2 = new Point();
Action GetPos1 = null;
Action GetPos2 = null;
KeyListener setupTracker = null;
OnScreenArea testArea = null;

GetPos1 = () =>
{
    testPos1 = System.Windows.Forms.Cursor.Position;
    Console.WriteLine($"Pos1: {testPos1.X}; {testPos1.Y}");
    setupTracker.StopListening();
    setupTracker = new KeyListener(GetPos2, InputKeys.A, false, true);
};

GetPos2 = () =>
{
    testPos2 = System.Windows.Forms.Cursor.Position;
    Console.WriteLine($"Pos1: {testPos2.X}; {testPos2.Y}");
    testArea = new OnScreenArea(testPos1, testPos2);
    setupTracker.StopListening();
};

SettingsManager.SetupSettings();

KeyListener testTracker = null;

InputHelper inputManager = new InputHelper();

Action GetText = () =>
{
    var screenAreaBitmap = ScreenHelper.GetScreenAreaBitmap(testArea);
    screenAreaBitmap = ScreenHelper.ResizeImage(screenAreaBitmap, 2);
    screenAreaBitmap = ScreenHelper.GetWhiteTextForReading(screenAreaBitmap);
    ScreenHelper.SaveBitmap(screenAreaBitmap);
    var text = ScreenHelper.GetTextInArea(testArea);
    Console.WriteLine($"Read text: {text}");
};

ScreenHelper.SetupScreenHelper();
setupTracker = new KeyListener(GetPos1, InputKeys.A, false, true);
testTracker = new KeyListener(GetText, InputKeys.W, true, false);
inputManager.Start();
Console.WriteLine($"Start");
*/

Application.Run();


