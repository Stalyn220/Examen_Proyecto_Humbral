using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;
using Umbral;
using UmbralEditor;

public class UmbralAuthoringTests
{
    [Test]
    public void NewRoomConnectsTerrainUpdatesCameraAndMovesPortal()
    {
        EditorSceneManager.OpenScene(UmbralBuilder.ScenePath);
        var game = Object.FindFirstObjectByType<UmbralGame>();
        var source = game.world;
        var copy = Object.Instantiate(source);
        game.world = copy;
        int count = copy.sectors.Length;
        float previousEnd = copy.maxX;
        var portal = Object.FindObjectsByType<UmbralProp>(FindObjectsSortMode.None).Single(p => p.kind == PropKind.Portal);
        var room = UmbralRoomAuthoring.AddRoom("PRUEBA", 32, true, true);
        Assert.That(copy.sectors.Length, Is.EqualTo(count + 1));
        Assert.That(copy.maxX, Is.EqualTo(previousEnd + 32));
        Assert.That(copy.sectors.Last().startX, Is.EqualTo(previousEnd));
        Assert.That(portal.transform.position.x, Is.GreaterThan(previousEnd));
        Assert.That(game.maraChannelPoint.position.x, Is.GreaterThan(previousEnd));
        foreach (var map in Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None))
            Assert.That(map.HasTile(new Vector3Int((int)previousEnd - 1, 1, 0)), Is.False, "La pared antigua no debe bloquear el acceso.");
        Assert.That(room.GetComponentInChildren<Tilemap>().HasTile(new Vector3Int((int)previousEnd + 1, -1, 0)), Is.True);
        // Do not save this temporary test room or the cloned world configuration.
        game.world = source;
        Object.DestroyImmediate(copy);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }
}
