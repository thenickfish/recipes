import React from "react";
import ReactDOM from "react-dom";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import CategoryHome from "./components/category/category_home";
import Dashboard from "./components/dashboard/dashboard";
import Recipe from "./components/recipe";

// const App = () => {
//   return (
//     <>
//       <Dashboard />
//     </>
//   );
// };

// document.addEventListener("DOMContentLoaded", () => {
//   const rootEl = document.getElementById("app");
//   ReactDOM.render(<App />, rootEl);
// });

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Dashboard />}>
          {/* <Route index element={<Home />} /> */}
          {/* <Route path="/categories/:category" element={<Main />} /> */}
          {/* <Route path="contact" element={<Contact />} /> */}
          {/* <Route path="*" element={<NoPage />} /> */}
        </Route>
        <Route path="/:categorySlug" element={<CategoryHome />} />
        <Route path="/:categorySlug/:recipeSlug" element={<Recipe />} />
      </Routes>
    </BrowserRouter>
  );
}
document.addEventListener("DOMContentLoaded", () => {
  const rootEl = document.getElementById("app");
  ReactDOM.render(<App />, rootEl);
});
