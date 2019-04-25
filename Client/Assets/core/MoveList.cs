using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class MoveList
    {
        float mLastMoveTimestamp;
        public List<Move> mMoves = new List<Move>();

        public MoveList()
        {

            mLastMoveTimestamp = -1.0f;
        }

        public Move	AddMove( InputState inInputState, float inTimestamp )
        {
            //first move has 0 time. it's okay, it only happens once
            float deltaTime = mLastMoveTimestamp >= 0.0f ? inTimestamp - mLastMoveTimestamp : 0.0f;

            mMoves.Add(new Move(inInputState, inTimestamp, deltaTime));

            mLastMoveTimestamp = inTimestamp;

            return mMoves.LastOrDefault();
        }
        public bool AddMoveIfNew(Move inMove)
        {
            //we might have already received this move in another packet ( since we're sending the same move in multiple packets )
            //so make sure it's new...

            //adjust the deltatime and then place!
            float timeStamp = inMove.GetTimestamp();

            if (timeStamp > mLastMoveTimestamp)
            {
                float deltaTime = mLastMoveTimestamp >= 0.0f ? timeStamp - mLastMoveTimestamp : 0.0f;

                mLastMoveTimestamp = timeStamp;

                mMoves.Add(new Move(inMove.GetInputState(), timeStamp, deltaTime));
                return true;
            }

            return false;

        }

		public	void RemovedProcessedMoves(float inLastMoveProcessedOnServerTimestamp)
        {
            while (mMoves.Count != 0 && mMoves[0].GetTimestamp() <= inLastMoveProcessedOnServerTimestamp)
            {
                mMoves.RemoveAt(0);
            }
        }

        public float GetLastMoveTimestamp() { return mLastMoveTimestamp; }

        public Move GetLatestMove() { return mMoves.LastOrDefault(); }

        public void Clear() { mMoves.Clear(); }
        public bool HasMoves() { return mMoves.Count != 0; }
        public int GetMoveCount() { return mMoves.Count; }

    }
}
