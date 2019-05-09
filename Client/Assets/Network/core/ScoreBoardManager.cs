using Lidgren.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using uint32_t = System.UInt32;

namespace core
{
    public class ScoreBoardManager
    {
        /// <summary>
        /// Global instance of ScoreBoardManager
        /// </summary>
        public static ScoreBoardManager sInstance = new ScoreBoardManager();

        public static void StaticInit()
        {
            sInstance = new ScoreBoardManager();
        }
        public List<Entry> GetEntries() { return mEntries; }

        ScoreBoardManager()
        {
            mDefaultColors.Add(Colors.LightYellow.Clone());
            mDefaultColors.Add(Colors.LightBlue.Clone());
            mDefaultColors.Add(Colors.LightPink.Clone());
            mDefaultColors.Add(Colors.LightGreen.Clone());
        }

        List<Entry> mEntries = new List<Entry>();

        List<Vector3> mDefaultColors = new List<Vector3>();

        public class Entry
        {
            public Entry() { }

            public Entry(uint32_t inPlayerID, string inPlayerName, Vector3 inColor)
            {
                mPlayerId = inPlayerID;
                mPlayerName = inPlayerName;
                mColor = inColor;
                SetScore(0);
            }

            public Vector3 GetColor() { return mColor; }
            public uint32_t GetPlayerId() { return mPlayerId; }
            public string GetPlayerName() { return mPlayerName; }
            public string GetFormattedNameScore() { return mFormattedNameScore; }
            public int GetScore() { return mScore; }

            public void SetScore(int inScore)
            {
                mScore = inScore;
                mFormattedNameScore = string.Format("{0} {1}", mPlayerName, mScore);
            }

            public bool Write(NetOutgoingMessage inOutputStream)
            {
                bool didSucceed = true;

                inOutputStream.Write(mColor);
                inOutputStream.Write(mPlayerId);
                inOutputStream.Write(mPlayerName);
                inOutputStream.Write(mScore);

                return didSucceed;
            }
            public bool Read(NetIncomingMessage inInputStream)
            {
                bool didSucceed = true;

                inInputStream.Read(mColor);
                mPlayerId = inInputStream.ReadUInt32();

                mPlayerName = inInputStream.ReadString();

                int score = inInputStream.ReadInt32();
                if (didSucceed)
                {
                    SetScore(score);
                }


                return didSucceed;
            }
            //public static uint32_t GetSerializedSize();

            Vector3 mColor = new Vector3();

            uint32_t mPlayerId;
            string mPlayerName;

            int mScore;

            string mFormattedNameScore;
        };


        public Entry GetEntry(uint32_t inPlayerId)
        {
            foreach (var entry in mEntries)
            {
                if (entry.GetPlayerId() == inPlayerId)
                {
                    return entry;
                }
            }

            return null;
        }

        public bool RemoveEntry(uint32_t inPlayerId)
        {
            foreach (var entry in mEntries)
            {
                if (entry.GetPlayerId() == inPlayerId)
                {
                    mEntries.Remove(entry);
                    return true;
                }
            }
            return false;
        }

        public void AddEntry(uint32_t inPlayerId, string inPlayerName)
        {
            //if this player id exists already, remove it first- it would be crazy to have two of the same id
            RemoveEntry(inPlayerId);

            mEntries.Add(new Entry(inPlayerId, inPlayerName, mDefaultColors[(int)(inPlayerId % mDefaultColors.Count)]));
        }

        public void IncScore(uint32_t inPlayerId, int inAmount)
        {
            Entry entry = GetEntry(inPlayerId);
            if (entry != null)
            {
                entry.SetScore(entry.GetScore() + inAmount);
            }
        }



        public bool Write(NetOutgoingMessage inOutputStream)
        {
            int entryCount = mEntries.Count;

            //we don't know our player names, so it's hard to check for remaining space in the packet...
            //not really a concern now though
            inOutputStream.Write(entryCount);
            foreach (var entry in mEntries)
            {
                entry.Write(inOutputStream);
            }

            return true;
        }


        public bool Read(NetIncomingMessage inInputStream)
        {
            int entryCount = inInputStream.ReadInt32();
            //just replace everything that's here, it don't matter...
            for (int i = 0; i < entryCount; ++i)
            {
                var en = new Entry();
                en.Read(inInputStream);
                mEntries.Add(en);
            }

            return true;
        }
    }
}
