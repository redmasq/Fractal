This is a rather basic Mandelbrot set generator. It is configurable in App.config. It was mainly just combination of practice and showing someone how to do something (use of Task<> and custom data structures, in this case Complex.cs) from about 4 years ago (2019) written using an older version of .Net. It uses a De Moivre Nth root finder, but I forget whether I grabbed an existing implementation from elsewhere or implemented it myself, but I did watch a few videos from 3Blue1Brown and blackpenredpen on YouTube, so could have been either. It works based on the ye olde trig identity e^(i*theta) = cos(theta) + i * sin(theta) since i^1, i^2, i^3, and i^4 does circles around the complex plane. So, if I shameless rip it off without credit, I apologize. If I did grab it, it's likely from somewhere like StackOverflow or CodeProject; although, I would have modified it to my liking anyways (at the very least, a search for my misspelling of reciprocal as "repcipical" didn't turn up anything, so maybe I did write it).

In the meantime, treat it as licensed under the usual open source licenses such as Mozilla or MIT.
