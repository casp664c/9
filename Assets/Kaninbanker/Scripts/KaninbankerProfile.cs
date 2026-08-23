using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Persistent progression for the larger mobile game. Kept deliberately local/offline so
    /// the prototype remains cloud-build friendly and does not require a backend to play.
    /// </summary>
    public sealed class KaninbankerProfile
    {
        private const string CoinsKey = "Kaninbanker.Profile.Coins";
        private const string XpKey = "Kaninbanker.Profile.Xp";
        private const string TotalHitsKey = "Kaninbanker.Profile.TotalHits";
        private const string TotalRoundsKey = "Kaninbanker.Profile.TotalRounds";
        private const string BestComboKey = "Kaninbanker.Profile.BestCombo";
        private const string TrophiesKey = "Kaninbanker.Profile.Trophies";

        public int Coins { get; private set; }
        public int Xp { get; private set; }
        public int TotalHits { get; private set; }
        public int TotalRounds { get; private set; }
        public int BestCombo { get; private set; }
        public int Trophies { get; private set; }

        public int Level => 1 + Xp / 450;
        public int LevelXp => Xp % 450;
        public float LevelProgress01 => Mathf.Clamp01(LevelXp / 450f);

        public string Rank
        {
            get
            {
                int level = Level;
                if (level >= 30) return "KANINKEJSER";
                if (level >= 20) return "GULDHAMMER";
                if (level >= 12) return "MEGABANKER";
                if (level >= 7) return "KANINJÆGER";
                if (level >= 3) return "HAMMERSVEND";
                return "NY BANKER";
            }
        }

        public void Load()
        {
            Coins = PlayerPrefs.GetInt(CoinsKey, 250);
            Xp = PlayerPrefs.GetInt(XpKey, 0);
            TotalHits = PlayerPrefs.GetInt(TotalHitsKey, 0);
            TotalRounds = PlayerPrefs.GetInt(TotalRoundsKey, 0);
            BestCombo = PlayerPrefs.GetInt(BestComboKey, 0);
            Trophies = PlayerPrefs.GetInt(TrophiesKey, 0);
        }

        public void AwardRound(int score, int hits, int bestCombo, float accuracy01, bool challengeCompleted)
        {
            int coinReward = Mathf.Max(10, score / 2 + hits * 2);
            int xpReward = Mathf.Max(20, score * 2 + Mathf.RoundToInt(accuracy01 * 80f));

            if (challengeCompleted)
            {
                coinReward += 100;
                xpReward += 150;
            }

            Coins += coinReward;
            Xp += xpReward;
            TotalHits += hits;
            TotalRounds += 1;
            BestCombo = Mathf.Max(BestCombo, bestCombo);

            CheckAchievement("FirstRound", TotalRounds >= 1);
            CheckAchievement("HundredHits", TotalHits >= 100);
            CheckAchievement("ComboTen", BestCombo >= 10);
            CheckAchievement("ScoreHundred", score >= 100);
            CheckAchievement("LevelTen", Level >= 10);
            Save();
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0 || Coins < amount)
                return false;

            Coins -= amount;
            Save();
            return true;
        }

        public void GrantCoins(int amount)
        {
            Coins += Mathf.Max(0, amount);
            Save();
        }

        private void CheckAchievement(string id, bool condition)
        {
            if (!condition)
                return;

            string key = "Kaninbanker.Achievement." + id;
            if (PlayerPrefs.GetInt(key, 0) != 0)
                return;

            PlayerPrefs.SetInt(key, 1);
            Trophies += 1;
            Coins += 75;
        }

        public void Save()
        {
            PlayerPrefs.SetInt(CoinsKey, Coins);
            PlayerPrefs.SetInt(XpKey, Xp);
            PlayerPrefs.SetInt(TotalHitsKey, TotalHits);
            PlayerPrefs.SetInt(TotalRoundsKey, TotalRounds);
            PlayerPrefs.SetInt(BestComboKey, BestCombo);
            PlayerPrefs.SetInt(TrophiesKey, Trophies);
            PlayerPrefs.Save();
        }
    }
}
