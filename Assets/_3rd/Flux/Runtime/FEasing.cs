// Easing Functions grabbed from iTween

// Copyright (c) 2011 Bob Berkebile (pixelplacment)
// Please direct any bugs/comments/suggestions to http://pixelplacement.com
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

/*
 TERMS OF USE - EASING EQUATIONS
 Open source under the BSD License.
 Copyright (c)2001 Robert Penner
 All rights reserved.
 Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using DG.Tweening;
using UnityEngine;
using XiaoCao;

namespace Flux
{
    public enum FEasingType
	{
        [EnumLabel("Linear")]
        Linear,
        //[EnumLabel("Clerp")]
        //Clerp,
        //[EnumLabel("Spring")]
        //Spring,
        [EnumLabel("加速")]
        EaseInQuad,
        [EnumLabel("减速")]
        EaseOutQuad,
        [EnumLabel("淡入淡出")]
        EaseInOutQuad
        #region 取消
        //[EnumLabel("EaseInCubic")]
        //EaseInCubic,
        //      [EnumLabel("EaseOutCubic")]
        //EaseOutCubic,
        //      [EnumLabel("EaseInOutCubic")]
        //      EaseInOutCubic,
        //      [EnumLabel("EaseInQuart")]
        //      EaseInQuart,
        //      [EnumLabel("EaseOutQuart")]
        //      EaseOutQuart,
        //      [EnumLabel("EaseInOutQuart")]
        //      EaseInOutQuart,
        //      [EnumLabel("EaseInQuint")]
        //      EaseInQuint,
        //      [EnumLabel("EaseOutQuint")]
        //      EaseOutQuint,
        //      [EnumLabel("EaseInOutQuint")]
        //      EaseInOutQuint,
        //      [EnumLabel("EaseInSine")]
        //      EaseInSine,
        //      [EnumLabel("EaseOutSine")]
        //EaseOutSine,
        //      [EnumLabel("EaseInOutSine")]
        //      EaseInOutSine,
        //      [EnumLabel("EaseInExpo")]
        //EaseInExpo,
        //      [EnumLabel("EaseOutExpo")]
        //      EaseOutExpo,
        //      [EnumLabel("EaseInOutExpo")]
        //      EaseInOutExpo,
        //      [EnumLabel("EaseInCirc")]
        //      EaseInCirc,
        //      [EnumLabel("EaseOutCirc")]
        //      EaseOutCirc,
        //      [EnumLabel("EaseInOutCirc")]
        //EaseInOutCirc,
        //      [EnumLabel("EaseInBounce")]
        //      EaseInBounce,
        //      [EnumLabel("EaseOutBounce")]
        //      EaseOutBounce,
        //      [EnumLabel("EaseInOutBounce")]
        //      EaseInOutBounce,
        //      [EnumLabel("EaseInBack")]
        //      EaseInBack,
        //      [EnumLabel("EaseOutBack")]
        //      EaseOutBack,
        //      [EnumLabel("EaseInOutBack")]
        //EaseInOutBack,
        //      [EnumLabel("EaseInElastic")]
        //      EaseInElastic,
        //      [EnumLabel("EaseOutElastic")]
        //      EaseOutElastic,
        //      [EnumLabel("EaseInOutElastic")]
        //      EaseInOutElastic
        #endregion

    }

    public static class FEasing
	{
		public static Ease ToDotweenEase(this FEasingType fEasing)
        {
			if (fEasing == FEasingType.Linear)
				return  Ease.Linear;
			else if (fEasing == FEasingType.EaseInQuad)
				return  Ease.InQuad;
			else if (fEasing == FEasingType.EaseOutQuad)
				return Ease.OutQuad;
			else if (fEasing == FEasingType.EaseInOutQuad)
				return Ease.InOutQuad;
            else
            {
				Debug.LogError("yns	fEasing ??" + fEasing);
				return Ease.Linear;
            }

		}

		private static System.Func<float, float, float, float>[] _tweens;

		static FEasing()
		{
			_tweens = new System.Func<float, float, float, float>[]{
				Linear,
				//Clerp,
				//Spring,
				EaseInQuad,
				EaseOutQuad,
				EaseInOutQuad
#region 取消
                /*
				EaseInCubic,
				EaseOutCubic,
				EaseInOutCubic,
				EaseInQuart,
				EaseOutQuart,
				EaseInOutQuart,
				EaseInQuint,
				EaseOutQuint,
				EaseInOutQuint,
				EaseInSine,
				EaseOutSine,
				EaseInOutSine,
				EaseInExpo,
				EaseOutExpo,
				EaseInOutExpo,
				EaseInCirc,
				EaseOutCirc,
				EaseInOutCirc,
				EaseInBounce,
				EaseOutBounce,
				EaseInOutBounce,
				EaseInBack,
				EaseOutBack,
				EaseInOutBack,
				EaseInElastic,
				EaseOutElastic,
				EaseInOutElastic
*/
        #endregion
			};
		}

		public static float Tween( float start, float end, float t, FEasingType easingType )
		{
			return _tweens[(int)easingType].Invoke( start, end, t );
		}
        
		public static float Linear(float start, float end, float t)
		{
			end -= start;
			return end * t + start;
		}
		
		public static float Clerp(float start, float end, float t){
			float min = 0.0f;
			float max = 360.0f;
			float half = Mathf.Abs((max - min) * 0.5f);
			float retval = 0.0f;
			float diff = 0.0f;
			if ((end - start) < -half){
				diff = ((max - start) + end) * t;
				retval = start + diff;
			}else if ((end - start) > half){
				diff = -((max - end) + start) * t;
				retval = start + diff;
			}else retval = start + (end - start) * t;
			return retval;
		}
		
		public static float Spring(float start, float end, float t){
			t = Mathf.Clamp01(t);
			t = (Mathf.Sin(t * Mathf.PI * (0.2f + 2.5f * t * t * t)) * Mathf.Pow(1f - t, 2.2f) + t) * (1f + (1.2f * (1f - t)));
			return start + (end - start) * t;
		}
		
		public static float EaseInQuad(float start, float end, float t){
			end -= start;
			return end * t * t + start;
		}
		
		public static float EaseOutQuad(float start, float end, float t){
			end -= start;
			return -end * t * (t - 2) + start;
		}
		
		public static float EaseInOutQuad(float start, float end, float t){
			t /= .5f;
			end -= start;
			if (t < 1) return end * 0.5f * t * t + start;
			t--;
			return -end * 0.5f * (t * (t - 2) - 1) + start;
		}
		
		public static float EaseInCubic(float start, float end, float t){
			end -= start;
			return end * t * t * t + start;
		}
		
		public static float EaseOutCubic(float start, float end, float t){
			t--;
			end -= start;
			return end * (t * t * t + 1) + start;
		}
		
		public static float EaseInOutCubic(float start, float end, float t){
			t /= .5f;
			end -= start;
			if (t < 1) return end * 0.5f * t * t * t + start;
			t -= 2;
			return end * 0.5f * (t * t * t + 2) + start;
		}
		
		public static float EaseInQuart(float start, float end, float t){
			end -= start;
			return end * t * t * t * t + start;
		}
		
		public static float EaseOutQuart(float start, float end, float t){
			t--;
			end -= start;
			return -end * (t * t * t * t - 1) + start;
		}
		
		public static float EaseInOutQuart(float start, float end, float t){
			t /= .5f;
			end -= start;
			if (t < 1) return end * 0.5f * t * t * t * t + start;
			t -= 2;
			return -end * 0.5f * (t * t * t * t - 2) + start;
		}
		
		public static float EaseInQuint(float start, float end, float t){
			end -= start;
			return end * t * t * t * t * t + start;
		}
		
		public static float EaseOutQuint(float start, float end, float t){
			t--;
			end -= start;
			return end * (t * t * t * t * t + 1) + start;
		}
		
		public static float EaseInOutQuint(float start, float end, float t){
			t /= .5f;
			end -= start;
			if (t < 1) return end * 0.5f * t * t * t * t * t + start;
			t -= 2;
			return end * 0.5f * (t * t * t * t * t + 2) + start;
		}
		
		public static float EaseInSine(float start, float end, float t){
			end -= start;
			return -end * Mathf.Cos(t * (Mathf.PI * 0.5f)) + end + start;
		}
		
		public static float EaseOutSine(float start, float end, float t){
			end -= start;
			return end * Mathf.Sin(t * (Mathf.PI * 0.5f)) + start;
		}
		
		public static float EaseInOutSine(float start, float end, float t){
			end -= start;
			return -end * 0.5f * (Mathf.Cos(Mathf.PI * t) - 1) + start;
		}
		
		public static float EaseInExpo(float start, float end, float t){
			end -= start;
			return end * Mathf.Pow(2, 10 * (t - 1)) + start;
		}
		
		public static float EaseOutExpo(float start, float end, float t){
			end -= start;
			return end * (-Mathf.Pow(2, -10 * t ) + 1) + start;
		}
		
		public static float EaseInOutExpo(float start, float end, float t){
			t /= .5f;
			end -= start;
			if (t < 1) return end * 0.5f * Mathf.Pow(2, 10 * (t - 1)) + start;
			t--;
			return end * 0.5f * (-Mathf.Pow(2, -10 * t) + 2) + start;
		}
		
		public static float EaseInCirc(float start, float end, float t){
			end -= start;
			return -end * (Mathf.Sqrt(1 - t * t) - 1) + start;
		}
		
		public static float EaseOutCirc(float start, float end, float t){
			t--;
			end -= start;
			return end * Mathf.Sqrt(1 - t * t) + start;
		}
		
		public static float EaseInOutCirc(float start, float end, float t){
			t /= .5f;
			end -= start;
			if (t < 1) return -end * 0.5f * (Mathf.Sqrt(1 - t * t) - 1) + start;
			t -= 2;
			return end * 0.5f * (Mathf.Sqrt(1 - t * t) + 1) + start;
		}
		
		/* GFX47 MOD START */
		public static float EaseInBounce(float start, float end, float t){
			end -= start;
			float d = 1f;
			return end - EaseOutBounce(0, end, d-t) + start;
		}
		/* GFX47 MOD END */
		
		/* GFX47 MOD START */
		//public static float bounce(float start, float end, float t){
		public static float EaseOutBounce(float start, float end, float t){
			t /= 1f;
			end -= start;
			if (t < (1 / 2.75f)){
				return end * (7.5625f * t * t) + start;
			}else if (t < (2 / 2.75f)){
				t -= (1.5f / 2.75f);
				return end * (7.5625f * (t) * t + .75f) + start;
			}else if (t < (2.5 / 2.75)){
				t -= (2.25f / 2.75f);
				return end * (7.5625f * (t) * t + .9375f) + start;
			}else{
				t -= (2.625f / 2.75f);
				return end * (7.5625f * (t) * t + .984375f) + start;
			}
		}
		/* GFX47 MOD END */
		
		/* GFX47 MOD START */
		public static float EaseInOutBounce(float start, float end, float t){
			end -= start;
			float d = 1f;
			if (t < d* 0.5f) return EaseInBounce(0, end, t*2) * 0.5f + start;
			else return EaseOutBounce(0, end, t*2-d) * 0.5f + end*0.5f + start;
		}
		/* GFX47 MOD END */
		
		public static float EaseInBack(float start, float end, float t){
			end -= start;
			t /= 1;
			float s = 1.70158f;
			return end * (t) * t * ((s + 1) * t - s) + start;
		}
		
		public static float EaseOutBack(float start, float end, float t){
			float s = 1.70158f;
			end -= start;
			t = (t) - 1;
			return end * ((t) * t * ((s + 1) * t + s) + 1) + start;
		}
		
		public static float EaseInOutBack(float start, float end, float t){
			float s = 1.70158f;
			end -= start;
			t /= .5f;
			if ((t) < 1){
				s *= (1.525f);
				return end * 0.5f * (t * t * (((s) + 1) * t - s)) + start;
			}
			t -= 2;
			s *= (1.525f);
			return end * 0.5f * ((t) * t * (((s) + 1) * t + s) + 2) + start;
		}
		

		
		/* GFX47 MOD START */
		public static float EaseInElastic(float start, float end, float t){
			end -= start;
			
			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;
			
			if (t == 0) return start;
			
			if ((t /= d) == 1) return start + end;
			
			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}
			
			return -(a * Mathf.Pow(2, 10 * (t-=1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + start;
		}		
		/* GFX47 MOD END */
		
		/* GFX47 MOD START */
		//public static float elastic(float start, float end, float t){
		public static float EaseOutElastic(float start, float end, float t){
			/* GFX47 MOD END */
			//Thank you to rafael.marteleto for fixing this as a port over from Pedro's UnityTween
			end -= start;
			
			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;
			
			if (t == 0) return start;
			
			if ((t /= d) == 1) return start + end;
			
			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p * 0.25f;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}
			
			return (a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) + end + start);
		}		
		
		/* GFX47 MOD START */
		public static float EaseInOutElastic(float start, float end, float t){
			end -= start;
			
			float d = 1f;
			float p = d * .3f;
			float s = 0;
			float a = 0;
			
			if (t == 0) return start;
			
			if ((t /= d*0.5f) == 2) return start + end;
			
			if (a == 0f || a < Mathf.Abs(end)){
				a = end;
				s = p / 4;
			}else{
				s = p / (2 * Mathf.PI) * Mathf.Asin(end / a);
			}
			
			if (t < 1) return -0.5f * (a * Mathf.Pow(2, 10 * (t-=1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p)) + start;
			return a * Mathf.Pow(2, -10 * (t-=1)) * Mathf.Sin((t * d - s) * (2 * Mathf.PI) / p) * 0.5f + end + start;
		}		
		/* GFX47 MOD END */

		public static float Punch(float amplitude, float t){
			float s = 9;
			if (t == 0){
				return 0;
			}
			else if (t == 1){
				return 0;
			}
			float period = 1 * 0.3f;
			s = period / (2 * Mathf.PI) * Mathf.Asin(0);
			return (amplitude * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 1 - s) * (2 * Mathf.PI) / period));
		}

	}	
}
