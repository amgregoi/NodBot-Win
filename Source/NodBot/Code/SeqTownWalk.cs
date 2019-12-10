using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class SeqTownWalk : SeqBase
    {
        private bool mTravelNorth = false;

        /// <summary>
        /// These points represent the min and max values to determine where the
        /// user is in accordance to the specified towns.
        /// </summary>
        private Point mTown5Min = new Point(329, 322);
        private Point mTown5Max = new Point(429, 352);
        private Point mTown4Min = new Point(347, 315);
        private Point mTown4Max = new Point(447, 315);

        /// <summary>
        /// This is the constructor fo the Town Walking Sequence.
        /// 
        /// </summary>
        /// <param name="aLogger"></param>
        public SeqTownWalk(Logger aLogger) : base(aLogger)
        {
            // Relevant constructor operations done in base class.
        }

        /// <summary>
        /// This is the public Start call, that also sets the intended direction of 
        /// the Town Walking sequence.
        /// 
        /// </summary>
        /// <param name="aCt"></param>
        /// <param name="aDirection"></param>
        /// <returns></returns>
        public async Task Start(CancellationToken aCt, bool aDirection)
        {
            mTravelNorth = aDirection;
            await Start(aCt);
        }

        /// <summary>
        /// This function runs the town walking sequence between town4 and town5.
        /// 
        /// Note: If this method is called directly, the direction 
        /// defaults to south (T4).
        /// </summary>
        /// <param name="aCt"></param>
        /// <returns></returns>
        public override async Task Start(CancellationToken aCt)
        {
            Point? aTown = null, aPrev = null;
            bool seenT4 = false, seenT5 = false;

            while (true)
            {
                aCt.ThrowIfCancellationRequested();

                if (mTravelNorth)
                {
                    aTown = mImageAnalyze.getMatchCoord(NodImages.Town5, NodImages.CurrentSS);
                    if (!seenT5) seenT5 = aTown != null;

                    if (seenT5 && aTown == null && aPrev.Value.X > mTown5Max.X)
                    {
                        mTravelNorth = !mTravelNorth;
                        seenT5 = false;
                        mTownSeenLast = false;
                        continue;
                    }

                    if (aTown == null) makeCorrectMove(new Point(), false);
                    else makeCorrectMove(aTown.Value, true);
                }
                else
                {
                
                    aTown = mImageAnalyze.getMatchCoord(NodImages.Town4, NodImages.CurrentSS);
                    if (!seenT4) seenT4 = aTown != null;

                    if (seenT4 && aTown == null && mTownSeenLast)
                    {
                        mTravelNorth = !mTravelNorth;
                        seenT4 = false;
                        mTownSeenLast = false;
                        continue;
                    }

                    if (aTown == null) makeCorrectMove(new Point(), false);
                    else makeCorrectMove(aTown.Value, true);
                }

                await checkForCombat();
                if (aTown != null)
                    aPrev = aTown;
            }
        }

        /// <summary>
        /// This function checks for combat after moving, and if combat has started
        /// attacks and exits accordingly to continue town walking.
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task checkForCombat()
        {
            await delay(1750);

            if (mImageAnalyze.ContainsMatch(NodImages.InCombat, NodImages.CurrentSS))
            {
                mInput.SettingsAttack();
                await delay(1000);

                while (!mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS)) ;

                while(mImageAnalyze.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
                {
                    mInput.Exit();
                    await delay(1000);
                }

                await delay(3500);
            }

        }

        bool mTownSeenLast = false;
        /// <summary>
        /// This function decides which direction to move based on the specified point
        /// and which direction we are trying to travel towards.
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="aTownSeen"></param>
        private void makeCorrectMove(Point p, bool aTownSeen)
        {
           
            if (mTravelNorth)
            {
                if (aTownSeen)
                {
                    if (p.Y < mTown5Min.Y) mInput.moveUp();
                    else if (p.Y >= mTown5Max.Y) mInput.moveDown();
                    else
                    {
                        if (p.X >= mTown5Max.X) mInput.moveRight();
                        else if (p.X <= mTown5Min.X) mInput.moveLeft();
                        else mTravelNorth = !mTravelNorth;
                    }
                }
                else if(mTownSeenLast && !aTownSeen)
                {
                    mTravelNorth = !mTravelNorth;
                }
                else
                {
                    mInput.moveUp();
                }
            }
            else
            {
                if (aTownSeen)
                {
                    if (p.Y < mTown4Min.Y) mInput.moveUp();
                    else if (p.Y > mTown4Min.Y) mInput.moveDown();
                    else
                    {
                        if (p.X <= mTown4Min.X) mInput.moveLeft();
                        else if (p.X >= mTown4Max.X) mInput.moveRight();
                        else mTravelNorth = !mTravelNorth;
                    }
                }
                else if (mTownSeenLast && !aTownSeen)
                {
                    mTravelNorth = !mTravelNorth;
                }
                else
                {
                    mInput.moveDown();
                }
            }
            mTownSeenLast = aTownSeen;
        }
    }
}
