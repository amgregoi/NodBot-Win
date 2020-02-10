﻿using NodBot.Code.Services;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace NodBot.Code
{
    public class SeqTownWalk : SeqBase
    {
        private bool mTravelNorth = false;
        private TimeService timeService;

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
        /// <param name="logger"></param>
        public SeqTownWalk(CancellationTokenSource ct, Logger logger) : base(ct, logger)
        {
            timeService = new TimeService(logger);
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
        public async Task Start(bool aDirection)
        {
            mTravelNorth = aDirection;
            await Start();
        }

        /// <summary>
        /// This function runs the town walking sequence between town4 and town5.
        /// 
        /// Note: If this method is called directly, the direction 
        /// defaults to south (T4).
        /// </summary>
        /// <param name="aCt"></param>
        /// <returns></returns>
        public override async Task Start()
        {
            Point? aTown = null, aPrev = null;
            bool seenT4 = false, seenT5 = false;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                if (mTravelNorth)
                {
                    aTown = imageService.getMatchCoord(NodImages.Town5, NodImages.CurrentSS);
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
                
                    aTown = imageService.getMatchCoord(NodImages.Town4, NodImages.CurrentSS);
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
            timeService.delay(1750);

            if (imageService.ContainsMatch(NodImages.InCombat, NodImages.CurrentSS))
            {
                nodInputService.SettingsAttack();

                timeService.delay(1000);

                while (!imageService.ContainsMatch(NodImages.Exit, NodImages.CurrentSS)) ;

                while(imageService.ContainsMatch(NodImages.Exit, NodImages.CurrentSS))
                {
                    nodInputService.Exit();
                    timeService.delay(1000);
                }

                timeService.delay(3500);
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
                    if (p.Y < mTown5Min.Y) nodInputService.moveUp();
                    else if (p.Y >= mTown5Max.Y) nodInputService.moveDown();
                    else
                    {
                        if (p.X >= mTown5Max.X) nodInputService.moveRight();
                        else if (p.X <= mTown5Min.X) nodInputService.moveLeft();
                        else mTravelNorth = !mTravelNorth;
                    }
                }
                else if(mTownSeenLast && !aTownSeen)
                {
                    mTravelNorth = !mTravelNorth;
                }
                else
                {
                    nodInputService.moveUp();
                }
            }
            else
            {
                if (aTownSeen)
                {
                    if (p.Y < mTown4Min.Y) nodInputService.moveUp();
                    else if (p.Y > mTown4Min.Y) nodInputService.moveDown();
                    else
                    {
                        if (p.X <= mTown4Min.X) nodInputService.moveLeft();
                        else if (p.X >= mTown4Max.X) nodInputService.moveRight();
                        else mTravelNorth = !mTravelNorth;
                    }
                }
                else if (mTownSeenLast && !aTownSeen)
                {
                    mTravelNorth = !mTravelNorth;
                }
                else
                {
                    nodInputService.moveDown();
                }
            }
            mTownSeenLast = aTownSeen;
        }
    }
}