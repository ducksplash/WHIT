var fso = new ActiveXObject("Scripting.FileSystemObject");
var path = WScript.Arguments(0);

function readUtf8(p) {
    var stream = new ActiveXObject("ADODB.Stream");
    stream.Type = 2;
    stream.Charset = "utf-8";
    stream.Open();
    stream.LoadFromFile(p);
    var text = stream.ReadText();
    stream.Close();
    return text;
}

function writeUtf8(p, text) {
    var stream = new ActiveXObject("ADODB.Stream");
    stream.Type = 2;
    stream.Charset = "utf-8";
    stream.Open();
    stream.WriteText(text);
    stream.SaveToFile(p, 2);
    stream.Close();
}

function stripComments(input) {
    var s = input.replace(/\r\n/g, "\n").replace(/\r/g, "\n");
    var out = [];
    var inString = false;
    var inChar = false;
    var inVerbatim = false;
    var inLineComment = false;
    var inBlockComment = false;

    for (var i = 0; i < s.length; i++) {
        var ch = s.charAt(i);
        var next = i + 1 < s.length ? s.charAt(i + 1) : "";

        if (inLineComment) {
            if (ch === "\n") {
                inLineComment = false;
                out.push("\n");
            }
            continue;
        }

        if (inBlockComment) {
            if (ch === "*" && next === "/") {
                inBlockComment = false;
                i++;
            }
            continue;
        }

        if (inString) {
            out.push(ch);
            if (inVerbatim) {
                if (ch === '"' && next === '"') {
                    out.push(next);
                    i++;
                } else if (ch === '"') {
                    inString = false;
                    inVerbatim = false;
                }
            } else {
                if (ch === "\\" && next !== "") {
                    out.push(next);
                    i++;
                } else if (ch === '"') {
                    inString = false;
                }
            }
            continue;
        }

        if (inChar) {
            out.push(ch);
            if (ch === "\\" && next !== "") {
                out.push(next);
                i++;
            } else if (ch === "'") {
                inChar = false;
            }
            continue;
        }

        if (ch === "/" && next === "/") {
            inLineComment = true;
            i++;
            continue;
        }

        if (ch === "/" && next === "*") {
            inBlockComment = true;
            i++;
            continue;
        }

        if (ch === '"') {
            inString = true;
            inVerbatim = i > 0 && s.charAt(i - 1) === "@";
            out.push(ch);
            continue;
        }

        if (ch === "'") {
            inChar = true;
            out.push(ch);
            continue;
        }

        out.push(ch);
    }

    return out.join("");
}

function compactBlankLines(input) {
    var lines = input.split("\n");
    var result = [];
    var blankCount = 0;

    for (var i = 0; i < lines.length; i++) {
        var line = lines[i].replace(/[ \t]+$/, "");
        if (/^[ \t]*$/.test(line)) {
            blankCount++;
            if (blankCount <= 1) {
                result.push("");
            }
        } else {
            blankCount = 0;
            result.push(line);
        }
    }

    return result.join("\r\n");
}

var content = readUtf8(path);
var stripped = stripComments(content);
var compacted = compactBlankLines(stripped);
writeUtf8(path, compacted);
