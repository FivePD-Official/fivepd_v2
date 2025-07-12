const express = require("express");
const path = require("path");

const app = express();
const PORT = 3000;

app.get("/", (_, res) => res.send(""));

app.use(
  "/audio",
  express.static(
    path.join(__dirname, "../../src/FivePD.Nui/src/assets/audio")
  )
);

app.listen(PORT, () => {
  console.log(`Server is running at http://localhost:${PORT}`);
});
