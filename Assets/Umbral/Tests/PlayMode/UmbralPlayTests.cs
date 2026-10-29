using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Umbral;

public sealed class UmbralPlayTests
{
    Keyboard keyboard;
    UmbralGame game;
    UmbralPlayer player;
    InputSettings.BackgroundBehavior previousBackgroundBehavior;
    InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Umbral");
        yield return null;
        previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
        previousEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        keyboard = InputSystem.AddDevice<Keyboard>();
        keyboard.MakeCurrent();
        InputSystem.onEvent += IgnoreOtherDevices;
        game = UmbralGame.Instance;
        player = game.player;
        Assert.That(game.Mode, Is.EqualTo(GameMode.Title));
        game.StartGame();
        yield return new WaitForSeconds(.25f);
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1;
        InputSystem.onEvent -= IgnoreOtherDevices;
        if (keyboard != null) InputSystem.RemoveDevice(keyboard);
        InputSystem.settings.backgroundBehavior = previousBackgroundBehavior;
        InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
        yield return null;
    }

    void IgnoreOtherDevices(InputEventPtr inputEvent, InputDevice device)
    {
        if (device != keyboard) inputEvent.handled = true;
    }

    IEnumerator Keys(float seconds, params Key[] keys)
    {
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        yield return new WaitForSecondsRealtime(seconds);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
    }

    IEnumerator Place(float x, float y)
    {
        player.Body.position = new Vector2(x, y);
        player.Body.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(.2f);
    }

    UmbralProp Prop(PropKind kind, int index = 0) => Object.FindObjectsByType<UmbralProp>(FindObjectsSortMode.None).Single(p => p.kind == kind && p.index == index);

    [UnityTest]
    public IEnumerator KeyboardMovementJumpPauseAndGravity()
    {
        Assert.That(player.Grounded, Is.True, "Leo debe apoyarse en el suelo.");
        float start = player.transform.position.x;
        yield return Keys(.5f, Key.D);
        Assert.That(player.transform.position.x, Is.GreaterThan(start + 1));
        float floor = player.transform.position.y;
        yield return Keys(.15f, Key.Space);
        Assert.That(player.transform.position.y, Is.GreaterThan(floor + .4f));
        yield return new WaitForSeconds(.9f);
        Assert.That(player.TryFlip(), Is.False, "Fuera de una zona Q debe rechazarse.");
        yield return Place(21, 1);
        Assert.That(player.InGravityZone, Is.True);
        yield return Keys(.05f, Key.Q);
        Assert.That(player.GravitySign, Is.EqualTo(-1));
        Assert.That(player.TryFlip(), Is.False, "La inversión debe respetar su recarga.");
        yield return new WaitForSeconds(1.3f);
        Assert.That(player.Grounded, Is.True, "Debe caminar apoyado en el techo.");
        Assert.That(player.transform.position.y, Is.GreaterThan(7));
        yield return Keys(.15f, Key.Space);
        Assert.That(player.Body.linearVelocity.y, Is.LessThan(0), "En el techo el salto debe dirigirse hacia abajo.");
        game.Pause(true);
        var pausedPosition = player.transform.position;
        yield return new WaitForSecondsRealtime(.2f);
        Assert.That(player.transform.position, Is.EqualTo(pausedPosition));
        game.Pause(false);
    }

    [UnityTest]
    public IEnumerator ProgressionGatesRescueAndVictory()
    {
        game.Win();
        Assert.That(game.Mode, Is.EqualTo(GameMode.Playing), "No se puede ganar antes del rescate.");
        Prop(PropKind.Reactor).Interact();
        Assert.That(game.Rescued, Is.False);
        Assert.That(Prop(PropKind.Door).barrier.enabled, Is.True);

        yield return Place(24.7f, 1);
        Assert.That(player.TryFlip(), Is.True);
        yield return new WaitForSeconds(1.3f);
        yield return Keys(.06f, Key.E);
        Assert.That(game.Modules[0], Is.True, "E debe recuperar el módulo del techo.");
        yield return null;
        Assert.That(Prop(PropKind.Door).barrier.enabled, Is.False);
        player.TryFlip();
        yield return new WaitForSeconds(1.3f);

        yield return Place(49, 5.3f);
        player.TryFlip();
        yield return new WaitForSeconds(.8f);
        yield return Keys(.06f, Key.E);
        Assert.That(game.Modules[1], Is.True);
        yield return null;
        Assert.That(Prop(PropKind.Door, 1).barrier.enabled, Is.True, "El módulo solo no abre la segunda puerta.");
        player.TryFlip();
        yield return Place(63, 1);
        yield return Keys(.06f, Key.E);
        yield return null;
        Assert.That(game.LeverActivated, Is.True);
        Assert.That(Prop(PropKind.Door, 1).barrier.enabled, Is.False);

        yield return Place(151, 1);
        player.TryFlip();
        yield return new WaitForSeconds(1.3f);
        foreach (var drone in Object.FindObjectsByType<UmbralDrone>(FindObjectsSortMode.None)) drone.Disable();
        yield return Place(158, 7.25f);
        yield return Keys(.06f, Key.E);
        Assert.That(game.ModuleCount, Is.EqualTo(3));
        player.TryFlip();
        yield return Place(174, 1);
        yield return Keys(.06f, Key.E);
        yield return null;
        Assert.That(game.Rescued, Is.True);
        Assert.That(Prop(PropKind.Door, 2).barrier.enabled, Is.False);
        yield return Place(214, 1);
        yield return Keys(.06f, Key.E);
        Assert.That(game.Mode, Is.EqualTo(GameMode.Won));
    }

    [UnityTest]
    public IEnumerator PulseDisablesDroneAndHazardPreservesModules()
    {
        game.Modules[0] = true;
        yield return Place(34, 1);
        yield return new WaitForSeconds(.2f);
        Assert.That(player.Checkpoint.x, Is.EqualTo(34).Within(.1f));
        Assert.That(player.TryFlip(), Is.True);
        yield return new WaitForSeconds(1.3f);
        yield return Keys(.05f, Key.D);
        var drone = Object.FindObjectsByType<UmbralDrone>(FindObjectsSortMode.None).OrderBy(d => d.left.x).First();
        drone.speed = 0;
        drone.transform.position = new Vector3(37, 7.15f, 0);
        Physics2D.SyncTransforms();
        yield return Keys(.06f, Key.J);
        Assert.That(player.TryAttack(), Is.False, "El ataque respeta su tiempo de recuperación.");
        yield return new WaitForSeconds(.25f);
        Assert.That(drone == null || drone.Disabled, Is.True, "El proyectil debe alcanzar y destruir al dron frontal.");
        player.Body.position = new Vector2(45, .7f);
        player.Body.linearVelocity = Vector2.down * 3;
        player.TryFlip();
        yield return new WaitForSeconds(.5f);
        Assert.That(game.Deaths, Is.EqualTo(1));
        Assert.That(player.transform.position.x, Is.EqualTo(34).Within(.25f));
        Assert.That(game.Modules[0], Is.True, "Los módulos se conservan al reaparecer.");
        Assert.That(player.GravitySign, Is.EqualTo(1));
        Assert.That(player.InGravityZone, Is.True, "Q sigue disponible al reaparecer dentro de una zona.");
    }

    [UnityTest]
    public IEnumerator CompleteSixSectorMissionUsingKeyboardWithoutTeleporting()
    {
        yield return MoveTo(10.6f);
        yield return Keys(.76f, Key.D, Key.LeftShift, Key.Space, Key.J);
        yield return MoveTo(24.8f);
        yield return FlipAndLand();
        yield return Keys(.06f, Key.E);
        Assert.That(game.ModuleCount, Is.EqualTo(1));
        yield return FlipAndLand();
        yield return MoveTo(35);
        yield return FlipAndLand();
        yield return MoveTo(49);
        yield return Keys(.06f, Key.E);
        Assert.That(game.ModuleCount, Is.EqualTo(2));
        yield return MoveTo(62);
        yield return FlipAndLand();
        yield return MoveTo(63);
        yield return Keys(.06f, Key.E);
        yield return MoveTo(74);
        yield return FlipAndLand();
        yield return MoveTo(99.5f);
        yield return FlipAndLand();
        yield return MoveTo(101);
        yield return Keys(.06f, Key.E);
        Assert.That(game.HasCircuit("cryo"), Is.True);
        yield return MoveTo(112);
        yield return FlipAndLand();
        yield return MoveTo(137.1f);
        yield return FlipAndLand();
        yield return Keys(1.3f, Key.J);
        Assert.That(game.HasCircuit("security"), Is.True, "J debe destruir el generador y abrir el acceso al reactor.");
        yield return MoveTo(151);
        yield return FlipAndLand();
        yield return MoveTo(158);
        yield return Keys(.06f, Key.E);
        Assert.That(game.ModuleCount, Is.EqualTo(3));
        yield return MoveTo(168);
        yield return FlipAndLand();
        yield return MoveTo(174);
        yield return Keys(.06f, Key.E);
        Assert.That(game.Rescued, Is.True);
        yield return MoveTo(214);
        yield return Keys(.06f, Key.E);
        Assert.That(game.Mode, Is.EqualTo(GameMode.Won));
        Assert.That(game.Deaths, Is.EqualTo(0), "La ruta completa debe ser transitable sin reinicios.");
        Assert.That(game.EnemiesDestroyed, Is.GreaterThanOrEqualTo(4));
    }
    IEnumerator FlipAndLand()
    {
        yield return Keys(.06f, Key.Q);
        yield return new WaitForSeconds(1.15f);
        Assert.That(player.Grounded, Is.True, "La inversión debe llevar a una superficie segura.");
    }

    IEnumerator MoveTo(float x)
    {
        float timeout = Time.realtimeSinceStartup + 12;
        InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.D, Key.LeftShift, Key.J));
        while (player.transform.position.x < x - .15f && Time.realtimeSinceStartup < timeout)
            yield return new WaitForSecondsRealtime(.025f);
        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;
        Assert.That(player.transform.position.x, Is.GreaterThan(x - .4f), "Recorrido bloqueado antes de x=" + x + " / modo=" + game.Mode + " / reinicios=" + game.Deaths + " / dirección=" + player.Horizontal + " / teclado=" + (Keyboard.current == keyboard));
    }

    IEnumerator DisableWithKeyboard(UmbralDrone drone)
    {
        for (int i = 0; i < 15 && !drone.Disabled; i++)
        {
            yield return Keys(.06f, Key.J);
            yield return new WaitForSeconds(.72f);
        }
        Assert.That(drone.Disabled, Is.True, "Debe poder despejarse el techo con pulsos frontales.");
    }

    [UnityTest]
    public IEnumerator ProjectileStopsAtWallAndCanBeFiredInAir()
    {
        yield return Place(34, 1);
        var drone = Object.FindObjectsByType<UmbralDrone>(FindObjectsSortMode.None).OrderBy(d => d.left.x).First();
        drone.speed = 0;
        drone.transform.position = new Vector3(38, player.transform.position.y, 0);
        var wall = new GameObject("Pared de prueba"); wall.layer = 6;
        wall.transform.position = new Vector3(36, 1.2f, 0);
        wall.AddComponent<BoxCollider2D>().size = new Vector2(.3f, 3);
        Physics2D.SyncTransforms();
        yield return Keys(.06f, Key.J);
        yield return new WaitForSeconds(.4f);
        Assert.That(drone != null && !drone.Disabled, Is.True, "El proyectil no atraviesa paredes.");
        Object.Destroy(wall);
        yield return null;
        yield return Keys(.1f, Key.Space);
        Assert.That(player.Grounded, Is.False);
        drone.transform.position = player.transform.position + Vector3.right * 3;
        Physics2D.SyncTransforms();
        yield return Keys(.06f, Key.J);
        yield return new WaitForSeconds(.25f);
        Assert.That(drone == null || drone.Disabled, Is.True, "El ataque en el aire debe causar daño real.");
    }

    [UnityTest]
    public IEnumerator ReinforcedEnemyRequiresTwoHitsAndIsDestroyed()
    {
        yield return Place(34, 1);
        var drone = Object.FindObjectsByType<UmbralDrone>(FindObjectsSortMode.None).First(d => d.GetComponent<UmbralDamageable>().maxHealth == 2);
        drone.speed = 0; drone.transform.position = new Vector3(37, player.transform.position.y, 0);
        Physics2D.SyncTransforms();
        var health = drone.GetComponent<UmbralDamageable>();
        yield return Keys(.06f, Key.J);
        yield return new WaitForSeconds(.17f);
        Assert.That(health.Health, Is.EqualTo(1));
        yield return new WaitForSeconds(.12f);
        yield return Keys(.06f, Key.J);
        yield return new WaitForSeconds(.3f);
        Assert.That(drone == null || drone.Disabled, Is.True);
        Assert.That(game.EnemiesDestroyed, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator ProfilesControlSpeedAndCharacterAppearance()
    {
        Assert.That(player.settings.walkSpeed, Is.GreaterThan(3));
        Assert.That(player.settings.runSpeed, Is.GreaterThan(5));
        var original = player.character;
        var profile = Object.Instantiate(original);
        profile.visualScale = 1.2f; profile.colliderSize = new Vector2(.8f, 1.6f);
        player.character = profile; player.ApplyCharacter();
        Assert.That(player.visual.transform.localScale.x, Is.EqualTo(1.2f));
        Assert.That(player.GetComponent<CapsuleCollider2D>().size, Is.EqualTo(profile.colliderSize));
        player.character = original; player.ApplyCharacter(); Object.Destroy(profile);
        yield return null;
    }

    [UnityTest]
    public IEnumerator RenderSixSectors()
    {
        string directory = Path.Combine(Application.dataPath, "..", "QA");
        Directory.CreateDirectory(directory);
        foreach (var item in new[] { new Vector2(8, 4), new Vector2(49, 4), new Vector2(87, 4), new Vector2(126, 4), new Vector2(174, 4), new Vector2(207, 4) })
        {
            var camera = Camera.main;
            camera.transform.position = new Vector3(item.x, item.y, -10);
            var target = new RenderTexture(1600, 900, 24);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var image = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            image.Apply();
            File.WriteAllBytes(Path.Combine(directory, "Sector_" + item.x + ".png"), image.EncodeToPNG());
            camera.targetTexture = null;
            RenderTexture.active = null;
            Object.Destroy(image);
            Object.Destroy(target);
        }
        yield return null;
    }
}
