#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Builds one runtime catalog from every usable Sprite and AudioClip under Assets/.
    /// Assets are classified by filename/path keywords so licensed Asset Store content can
    /// remain in its original folders. Package assets are intentionally not copied or redistributed.
    /// </summary>
    public static class Kaninbanker2DAssetCatalogBuilder
    {
        public const string CatalogDirectory = "Assets/Resources/Kaninbanker/Generated";
        public const string CatalogPath = CatalogDirectory + "/Kaninbanker2DAssetCatalog.asset";

        [MenuItem("Tools/Kaninbanker/Rebuild TRUE 2D Asset Catalog")]
        public static void BuildCatalog()
        {
            Directory.CreateDirectory(CatalogDirectory);
            AssetDatabase.Refresh();

            Kaninbanker2DAssetCatalog catalog = AssetDatabase.LoadAssetAtPath<Kaninbanker2DAssetCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<Kaninbanker2DAssetCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var normal = new List<Sprite>();
            var fast = new List<Sprite>();
            var gold = new List<Sprite>();
            var armored = new List<Sprite>();
            var bombs = new List<Sprite>();
            var bosses = new List<Sprite>();
            var holes = new List<Sprite>();
            var backgrounds = new List<Sprite>();
            var foregrounds = new List<Sprite>();
            var effects = new List<Sprite>();
            var ui = new List<Sprite>();
            var hammers = new List<Sprite>();
            var genericSprites = new List<Sprite>();

            var hit = new List<AudioClip>();
            var combo = new List<AudioClip>();
            var miss = new List<AudioClip>();
            var pop = new List<AudioClip>();
            var start = new List<AudioClip>();
            var gameOver = new List<AudioClip>();
            var uiAudio = new List<AudioClip>();
            var power = new List<AudioClip>();
            var bombAudio = new List<AudioClip>();
            var bossAudio = new List<AudioClip>();
            var reward = new List<AudioClip>();
            var music = new List<AudioClip>();
            var genericAudio = new List<AudioClip>();

            IndexSprites(normal, fast, gold, armored, bombs, bosses, holes, backgrounds,
                foregrounds, effects, ui, hammers, genericSprites);
            IndexAudio(hit, combo, miss, pop, start, gameOver, uiAudio, power, bombAudio,
                bossAudio, reward, music, genericAudio);

            catalog.normalRabbits = normal.ToArray();
            catalog.fastRabbits = fast.ToArray();
            catalog.goldRabbits = gold.ToArray();
            catalog.armoredRabbits = armored.ToArray();
            catalog.bombRabbits = bombs.ToArray();
            catalog.bossRabbits = bosses.ToArray();
            catalog.holes = holes.ToArray();
            catalog.backgrounds = backgrounds.ToArray();
            catalog.foregrounds = foregrounds.ToArray();
            catalog.effects = effects.ToArray();
            catalog.ui = ui.ToArray();
            catalog.hammers = hammers.ToArray();
            catalog.genericSprites = genericSprites.ToArray();

            catalog.hit = hit.ToArray();
            catalog.combo = combo.ToArray();
            catalog.miss = miss.ToArray();
            catalog.rabbitPop = pop.ToArray();
            catalog.roundStart = start.ToArray();
            catalog.gameOver = gameOver.ToArray();
            catalog.uiAudio = uiAudio.ToArray();
            catalog.power = power.ToArray();
            catalog.bomb = bombAudio.ToArray();
            catalog.boss = bossAudio.ToArray();
            catalog.reward = reward.ToArray();
            catalog.music = music.ToArray();
            catalog.genericAudio = genericAudio.ToArray();

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();

            int spriteCount = normal.Count + fast.Count + gold.Count + armored.Count + bombs.Count + bosses.Count +
                              holes.Count + backgrounds.Count + foregrounds.Count + effects.Count + ui.Count + hammers.Count + genericSprites.Count;
            int audioCount = hit.Count + combo.Count + miss.Count + pop.Count + start.Count + gameOver.Count + uiAudio.Count +
                             power.Count + bombAudio.Count + bossAudio.Count + reward.Count + music.Count + genericAudio.Count;

            Debug.Log($"[Kaninbanker] TRUE-2D Asset Catalog rebuilt: {spriteCount} categorized sprite references, {audioCount} categorized audio references. Imported assets are referenced, never duplicated.");
        }

        private static void IndexSprites(
            List<Sprite> normal, List<Sprite> fast, List<Sprite> gold, List<Sprite> armored,
            List<Sprite> bombs, List<Sprite> bosses, List<Sprite> holes, List<Sprite> backgrounds,
            List<Sprite> foregrounds, List<Sprite> effects, List<Sprite> ui, List<Sprite> hammers,
            List<Sprite> generic)
        {
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets" });
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldSkip(path))
                    continue;

                UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
                foreach (UnityEngine.Object obj in assets)
                {
                    if (!(obj is Sprite sprite))
                        continue;
                    string unique = path + "::" + sprite.name;
                    if (!seen.Add(unique))
                        continue;

                    string key = (path + "/" + sprite.name).ToLowerInvariant();
                    if (HasAny(key, "boss", "emperor", "kejser", "titan", "baron")) bosses.Add(sprite);
                    else if (HasAny(key, "bomb", "bombe", "explosive", "mine")) bombs.Add(sprite);
                    else if (HasAny(key, "armor", "armour", "armored", "armoured", "panser", "shieldrabbit")) armored.Add(sprite);
                    else if (HasAny(key, "gold", "golden", "guld", "jackpot")) gold.Add(sprite);
                    else if (HasAny(key, "fast", "speed", "runner", "sprinter", "hurtig")) fast.Add(sprite);
                    else if (HasAny(key, "rabbit", "bunny", "hare", "kanin")) normal.Add(sprite);
                    else if (HasAny(key, "hole", "burrow", "molehill", "hul")) holes.Add(sprite);
                    else if (HasAny(key, "background", "backdrop", "arena", "world", "environment", "bg_", "/bg")) backgrounds.Add(sprite);
                    else if (HasAny(key, "foreground", "frame", "overlay", "border")) foregrounds.Add(sprite);
                    else if (HasAny(key, "effect", "fx", "spark", "burst", "impact", "hit_", "explosion", "confetti")) effects.Add(sprite);
                    else if (HasAny(key, "hammer", "mallet", "club", "bonk")) hammers.Add(sprite);
                    else if (HasAny(key, "ui", "icon", "button", "badge", "medal", "coin", "panel", "hud")) ui.Add(sprite);
                    else generic.Add(sprite);
                }
            }
        }

        private static void IndexAudio(
            List<AudioClip> hit, List<AudioClip> combo, List<AudioClip> miss, List<AudioClip> pop,
            List<AudioClip> start, List<AudioClip> gameOver, List<AudioClip> ui, List<AudioClip> power,
            List<AudioClip> bomb, List<AudioClip> boss, List<AudioClip> reward, List<AudioClip> music,
            List<AudioClip> generic)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets" });
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (ShouldSkip(path))
                    continue;
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip == null || !seen.Add(path))
                    continue;

                string key = (path + "/" + clip.name).ToLowerInvariant();
                if (HasAny(key, "music", "theme", "loop", "soundtrack", "bgm")) music.Add(clip);
                else if (HasAny(key, "gameover", "game_over", "lose", "defeat")) gameOver.Add(clip);
                else if (HasAny(key, "roundstart", "round_start", "start", "ready", "go_")) start.Add(clip);
                else if (HasAny(key, "combo", "streak")) combo.Add(clip);
                else if (HasAny(key, "miss", "wrong", "fail")) miss.Add(clip);
                else if (HasAny(key, "rabbitpop", "rabbit_pop", "spawn", "pop")) pop.Add(clip);
                else if (HasAny(key, "bomb", "explosion", "boom")) bomb.Add(clip);
                else if (HasAny(key, "boss", "heavyhit", "heavy_hit")) boss.Add(clip);
                else if (HasAny(key, "reward", "win", "fanfare", "chest", "coin")) reward.Add(clip);
                else if (HasAny(key, "power", "powerup", "power_up", "boost")) power.Add(clip);
                else if (HasAny(key, "ui", "click", "tap", "select")) ui.Add(clip);
                else if (HasAny(key, "hit", "bonk", "whack", "smash", "impact")) hit.Add(clip);
                else generic.Add(clip);
            }
        }

        private static bool ShouldSkip(string path)
        {
            if (string.IsNullOrEmpty(path))
                return true;
            string p = path.Replace('\\', '/').ToLowerInvariant();
            return p.Contains("/editor/") || p.Contains("/tests/") || p.Contains("/samples~/") ||
                   p.StartsWith(CatalogDirectory.ToLowerInvariant() + "/");
        }

        private static bool HasAny(string value, params string[] needles)
        {
            for (int i = 0; i < needles.Length; i++)
            {
                if (value.Contains(needles[i]))
                    return true;
            }
            return false;
        }
    }
}
#endif
