import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import ProductList from "./components/ProductList";
import ProductDetail from "./components/ProductDetail";
import "./App.css";

function App() {
  return (
    <Router>
      <div className="app">
        <header className="app-header">
          <div className="header-content">
            <h1>Blogbox Store</h1>
            <p>Your E-Commerce Solution</p>
          </div>
        </header>

        <main className="app-main">
          <Routes>
            <Route path="/" element={<ProductList />} />
            <Route path="/products/:id" element={<ProductDetail />} />
          </Routes>
        </main>

        <footer className="app-footer">
          <p>&copy; 2024 Blogbox Store. Built with React & ASP.NET Core</p>
        </footer>
      </div>
    </Router>
  );
}

export default App;
