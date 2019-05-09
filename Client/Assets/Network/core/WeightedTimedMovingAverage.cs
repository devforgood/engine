using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace core
{
    public class WeightedTimedMovingAverage
    {
        public WeightedTimedMovingAverage(float inDuration = 5.0f)
        {
            mDuration = inDuration;
            mValue = 0.0f;

            mTimeLastEntryMade = Timing.sInstance.GetTimef();
        }

        public void UpdatePerSecond(float inValue)
        {
            float time = Timing.sInstance.GetTimef();
            float timeSinceLastEntry = time - mTimeLastEntryMade;

            float valueOverTime = inValue / timeSinceLastEntry;

            //now update our value by whatever amount of the duration that was..
            float fractionOfDuration = (timeSinceLastEntry / mDuration);
            if (fractionOfDuration > 1.0f) { fractionOfDuration = 1.0f; }

            mValue = mValue * (1.0f - fractionOfDuration) + valueOverTime * fractionOfDuration;

            mTimeLastEntryMade = time;
        }

        public void Update(float inValue)
        {
            float time = Timing.sInstance.GetTimef();
            float timeSinceLastEntry = time - mTimeLastEntryMade;

            //now update our value by whatever amount of the duration that was..
            float fractionOfDuration = (timeSinceLastEntry / mDuration);
            if (fractionOfDuration > 1.0f) { fractionOfDuration = 1.0f; }

            mValue = mValue * (1.0f - fractionOfDuration) + inValue * fractionOfDuration;

            mTimeLastEntryMade = time;
        }

        public float GetValue() { return mValue; }


        float mTimeLastEntryMade;
        float mValue;
        float mDuration;

    }
}
