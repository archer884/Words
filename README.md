Words
=====

The basic idea here is to simplify finding words matching a given pattern. Previously, I had a Powershell script that would print every word in my dictionary to the screen and I could search that using Select-String. The issue you run into there is pretty simple: how do you deal with letters you know aren't present?

In a regex, the solution is an exclusive group: `[^not]`. So instead of putting in `.` for letters you don't know, you put in a group including all the letters you *do* know. But do you like retyping that? Over and over and over again? And then modifying every group like a jackass every time you guess a new letter?

Hell no.

Words handles that stuff for you. Better still, it also handles *blanks*--those empty tiles you get in crossword-y games--which are almost impossible to deal with in good ol' fashioned regular expressions. And at the end of the day, you can still use Select-String (or grep, or grope, or... whatever the hell kids are doing nowadays) to filter output from Words, and you can even pipe that output back into Words for another go if you really want to. That's because I'm chill like that.

## Setup ##

Words consists of an executable (Words.exe), a config file (Words.exe.config), and a runtime (.NET, or Mono, or whatever--you should really already have one of the two installed, otherwise I don't know why you're here). Stick the exe and the config file together on your disk wherever you like and you're done installing. Just make sure you put them both in the same place.

The config file is standard .NET xml config stuff. I'm not going to explain that. It's too painful. I could have done it differently, but... Eh, screw it.

I went with config for this (instead of using command line arguments/flags/etc.) because Words is not really meant to be used in scripts. It's a lively, interactive kind of program: you're going to be typing the input yourself, so I have worked to keep it as short as possible. I may eventually add arguments that allow users to override min and max lengths, but I have not found that to be useful.

(It is already possible to override the word list being used: you just pipe any list you like into Words and use Filter mode, discussed below.)

Anyway, inside the config file there are really just three lines that matter. Here they are:

**WordListPath**  
The *WordListPath* entry tells Words where to look for your words. Your word bank should be a directory containing files containing nothing but words, one per line. For an example, go download `Enable1.txt` from... from pretty much anywhere on the whole internet, really. All files in this directory will be loaded when Words runs. Don't worry about duplicate words; each one will only get loaded once.

**MinimumLength**  
Most games allow words of X letters or more. Change '4' to X, whatever X happens to be. Words shorter than X will not be loaded when Words runs and will not appear in any outputs. Easy.

**MaximumLength**  
Hangman-style games often impose a maximum length for words (Hanging With Friends is 8), and this config option allows you to set that. You'll want this to be longer, probably, if you mean to get crazy with words in Scrabble-style games, because combining two or more words can result in the final word being longer than 8 characters. 

## Usage ##

Words knows five (five? I think it's five... You can read the source code if you really want to know, ok?) commands:

1. Get
2. Query
3. Filter
4. Add
5. Remove

Ok, it's four. I haven't added the fifth yet. Sorry. Whatever. Anyway, if you run Words without any arguments, all it does is print its word banks to the screen--which is handy if you also want to be able to Select-String/grep/grope/molest your word bank instead of, you know, searching it with Words.

Anyway, lemme show you how to use the commands.

** Note: All of these examples assume you've added an alias for Words, e.g. `Set-Alias words ~/bin/Words.exe` **

### Get
> "I need a word for Hangman because this guy is eating my shorts."

No problem. Here's your query: `words get werfsdfweoifskdk` You should probably replace *werfsdfweoifskdk* with whatever your actual letters are, though, because otherwise I'm not sure you're going to get good results. The output of the Get command is just whatever words can actually be created using the letters you provided, so this will also work for Scrabble, Words With Friends, et al.

> Shorthand: `words werfsdfweoifskdk`

### Query
> "What the hell is this? This is not English!"

Got you covered. Try: `words query .pp.e` Of course, don't try .pp.e unless you have two blanks, two Ps, and an E, in that order, because pretty much the only result that will give you is *apple,* which is only handy if your opponent has just chosen the word *apple.* Um... Well, you knew that. I hope.

> Shorthand: `words /.pp.e`

### Filter
> "None of these words will fit on my Scrabble game. >.<"

Calm down. Use: `words [original query] | words filter .i.e` where i and e are the existing letters you need to fit with. It's really not a big deal, I promise; you're just piping the output of words back into words: the first command makes the list of stuff and the second command filters it. You can actually achieve *basically* the same thing with any of your regex utilities on the pipeline, but Words still has that smart syntax for exclusive groups that I mentioned earlier, so it can still be useful.

### Add
> "How could you not include this word in your list?! Asshole..."

Technically, that's not my list. That's yours. I didn't provide you with a list--just with a program that searches the lists you provide. But if you want to really quickly add a word to those lists, you just use `words add words that are not in the list` and Words will append all of those to its own supplemental list stored in the same place as all the others.

### Remove
> "Your remove command doesn't work. Why did you even mention it?"

I know it's not implemented yet, but the idea is that--someday--I will add a command that allows Words to blacklist certain words by appending them to a blacklist file in the same wordbank directory as all the other files. This lets you 'delete' a word without actually modifying any other lists. 

## Advanced ##

Of course you realize everyone's going to accuse you of cheating when you use this (so don't tell them!) even though they use Google search for exactly the same reasons ("All I did was type iin the first three letters and it popped up--that's not cheating, is it?"), but the fact the remains that using this program to *win* is a skill that you're going to have to develop. It's almost as hard as just playing the damn game. Honest!

(Ok, no it's not. Whatever.)

Either way, I'll just cover a few topics here that may come in handy.

### Exclusions

Of course you remember that the entire point of *Query* mode is smart exclusions, but I only covered half of that above: any letter appearing in your query cannot appear in any of the wildcard positions in your query. That's handy because, when you guess a letter in Hangman or Wheel of Fortune, or whatever, *all* of a given letter will show up at once. But what about the letters you guess wrong?

**To add arbitrary exclusions to a query:**

> `words /.pp.. rstgo`  
> Prints words matching the query but not containing the letters appearing after the query

This exclusion syntax does not work with *Get.* If you don't want words including a given letter to appear in your output, just don't include that letter in your input. (Neat, huh? Yeah, it took me awhile to think of that, too...)

### Blanks

*Get* mode is capable of handling blanks. The syntax for this is different from normal get mode: `words get 2/ppl` Don't ask how it works. (Actually, the code involved WordMapExtensions.Contains() and you can look if you want: it's two lines, but that includes a tiny bit of linq, so you may not want to peek if you're feeling particularly Enterprise today.) Of course, Get mode does no filtering, so this is one of those where you'll need to pipe the output either back into words or into some other regular expression utility.

### Regular expressions

It may seem weird that I'm discussing regular expressions when I have written a utility intended to (mostly) replace them, but there is something to be said for doing things the hard way. In this case, that something is that the hard way is *easier.* Specifically, I chose not to reimplement all of this in Words because it's so simple to do it using [insert utility here] that it would have been a waste of my time to code it and a waste of your time (you already know how to grep/awk/sls, right?) to learn it all over again. 

**To show only words that start with a given letter:**

> `words 2/ppl | sls ^a`  
> Prints words starting with A, like 'apple'

**To show only words that end with a given letter:**

> `words 2/ppl | sls e$`  
> Prints words ending with E, also like 'apple'

**To use a specific letter as an anchor point:**

> `words 2/ppl | sls ^.p`  
> `words 2/ppl | sls l.$`  
> Prints words with a P as the second letter or (second example) with an L as the next-to-last letter

These examples use a pretty basic subset of the whole idea of regular expressions: letters are letters, dots are wildcards, and the symbols ^ and $ represent the beginning and end of a word, respectively. These strategies are mostly handy for scrabble, since interaction with other words on the board is key.
