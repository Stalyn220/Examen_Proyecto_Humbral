using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Umbral
{
    // Automatiza capturas de pantalla para revision visual o QA de una compilacion.
    // Solo se ejecuta cuando el programa recibe -umbral-qa seguido de una carpeta de salida.
    public sealed class UmbralCapture : MonoBehaviour
    {
        // Ejecuta una secuencia controlada de estados del juego y guarda las capturas en disco.
        IEnumerator Start()
        {
            var arguments = Environment.GetCommandLineArgs();
            int flag = Array.IndexOf(arguments, "-umbral-qa");
            if (flag < 0 || flag + 1 >= arguments.Length) yield break;
            string directory = arguments[flag + 1];
            Directory.CreateDirectory(directory);
            var game = UmbralGame.Instance;
            yield return new WaitForSecondsRealtime(1);
            yield return Capture(directory, "01_Menu.png");
            game.StartGame();
            yield return new WaitForSecondsRealtime(.5f);
            yield return Capture(directory, "02_Despertar.png");
            game.Modules[0] = true;
            game.player.Body.position = new Vector2(49, 5.8f);
            game.player.Body.linearVelocity = Vector2.zero;
            yield return new WaitForSecondsRealtime(.3f);
            game.player.TryFlip();
            yield return new WaitForSecondsRealtime(1);
            game.cameraRig.Snap();
            yield return Capture(directory, "03_Conductos.png");
            game.player.Respawn(false);
            game.Modules[1] = game.Modules[2] = true;
            game.LeverActivated = true;
            game.player.Body.position = new Vector2(174, 1);
            game.player.Body.linearVelocity = Vector2.zero;
            yield return new WaitForSecondsRealtime(.5f);
            game.Rescue();
            yield return new WaitForSecondsRealtime(2);
            game.cameraRig.Snap();
            yield return Capture(directory, "04_Reactor.png");
            game.Win();
            yield return Capture(directory, "05_Final.png");
            File.WriteAllText(Path.Combine(directory, "Captura_completada.txt"), "Cinco capturas de la compilación Windows. Secuencia de revisión visual; no es una prueba de recorrido.");
            Application.Quit(0);
        }

        // Espera a que Unity escriba cada captura antes de continuar con la secuencia.
        static IEnumerator Capture(string directory, string name)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(directory, name));
            yield return new WaitForSecondsRealtime(.85f);
        }
    }
}
