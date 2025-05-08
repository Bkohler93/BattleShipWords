package main

import (
	"bufio"
	"fmt"
	"log"
	"os"
	"slices"
)

func main() {
	bigWordList := getWords("bigwordlist.txt")
	threeLetterWordlist := getWords("threeletters.txt")
	fourLetterWordList := getWords("fourletters.txt")
	fiveLetterWordList := getWords("fiveletters.txt")

	for i, word := range threeLetterWordlist {
		if !slices.Contains(bigWordList, word) {
			threeLetterWordlist = slices.Replace(threeLetterWordlist, i, i+1, "")
			i--
		}
	}

	for i, word := range fourLetterWordList {
		if !slices.Contains(bigWordList, word) {
			fourLetterWordList = slices.Replace(fourLetterWordList, i, i+1, "")
			i--
		}
	}

	for i, word := range fiveLetterWordList {
		if !slices.Contains(bigWordList, word) {
			fiveLetterWordList = slices.Replace(fiveLetterWordList, i, i+1, "")
			i--
		}
	}

	for _, word := range threeLetterWordlist {
		if word != "" && !slices.Contains(bigWordList, word) {
			log.Printf("%s not found", word)
		}
	}

	for _, word := range fourLetterWordList {
		if word != "" && !slices.Contains(bigWordList, word) {
			log.Printf("%s not found", word)
		}
	}

	for _, word := range fiveLetterWordList {
		if word != "" && !slices.Contains(bigWordList, word) {
			log.Printf("%s not found", word)
		}
	}

	writeWords(threeLetterWordlist, "threeletters.txt")
	writeWords(fourLetterWordList, "fourletters.txt")
	writeWords(fiveLetterWordList, "fiveletters.txt")
}

func writeWords(words []string, filename string) {
	f, err := os.Create(filename)
	if err != nil {
		log.Fatalf("trouble creating filename '%s' - %s", filename, err)
	}
	defer f.Close()

	for _, word := range words {
		if word != "" {
			fmt.Fprintln(f, word)
		}
	}
}

func getWords(filename string) []string {
	f, err := os.Open(filename)
	if err != nil {
		log.Fatalf("trouble opening filename '%s' - %s", filename, err)
	}
	bigWordList := []string{}
	s := bufio.NewScanner(f)
	for s.Scan() {
		t := s.Text()
		bigWordList = append(bigWordList, t)
	}
	f.Close()

	return bigWordList
}
