import * as React from "react";
import * as ReactDOM from "react-dom";
import { Timer } from "./components/counter";

const App = () => {
  return (
    <div>
      Hello, Rails 7!
      <Timer seconds={100} />
    </div>
  );
};

document.addEventListener("DOMContentLoaded", () => {
  const rootEl = document.getElementById("app");
  ReactDOM.render(<App />, rootEl);
});
