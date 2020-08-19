matches = [];
matchIndex = undefined;

function updateMatch() {
    var hometeam, awayteam, homescore, awayscore, attribute, divider;

    hometeam = '';
    awayteam = '';
    homescore = '';
    awayscore = '';
    divider = '';
    attribute = '';

	if (matchIndex !== undefined) {
        var match, i, length;

        match = matches[matchIndex];
        hometeam = match.HomeTeam;
        awayteam = match.AwayTeam;
        homescore = match.WonCount;
        awayscore = match.LostCount;
        divider = '-';

		if (homescore > awayscore) {
            attribute = "winning";
		} else if (homescore < awayscore) {
            attribute = "losing";
		} else {
            attribute = "";
		}
	}

    document.querySelector("#teams .home").textContent = hometeam;
    document.querySelector("#teams .away").textContent = awayteam;
    document.querySelector("#scores .home").textContent = homescore;
    document.querySelector("#scores .away").textContent = awayscore;
    document.querySelector("#scores .divider").textContent = divider;
    document.querySelector("#scores").setAttribute("class", attribute);
}

function nextMatch() {
	if (matches.length === 0) {
        matchIndex = undefined;
	} else {
        matchIndex = ++matchIndex % matches.length;
	}
    console.log("NEXT");
    updateMatch();
}

function updateMatches() {
    var request;
    request = new XMLHttpRequest();
    request.open("GET", "/api/matches.json", true);
    request.send();
	request.onreadystatechange = function() {
		if (request.readyState == 4 && request.status == 200) {
            matches = JSON.parse(request.responseText);
			if (matches.length === 0) {
                matchIndex = undefined;
			} else if (matchIndex === undefined) {
                matchIndex = 0;
			} else {
                matchIndex = matchIndex % matches.length;
			}
            updateMatch();
		}
	}
}

setInterval(nextMatch, 6000);
setInterval(updateMatches, 5000);

updateMatches();