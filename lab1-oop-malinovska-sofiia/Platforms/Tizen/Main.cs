using System;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace lab1_oop_malinovska_sofiia;

class Program : MauiApplication
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    static void Main(string[] args)
    {
        var app = new Program();
        app.Run(args);
    }
}