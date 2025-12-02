import { useNavigate } from "react-router-dom";
import "./CheckoutCancelled.css";

function CheckoutCancelled() {
  const navigate = useNavigate();

  return (
    <div className="cancelled-container">
      <div className="cancelled-content">
        <div className="cancelled-icon">✕</div>
        <h1>Payment Cancelled</h1>
        <p>
          Your payment was cancelled. Your cart items have been saved and you
          can try again when you're ready.
        </p>

        <div className="cancelled-actions">
          <button
            onClick={() => navigate("/checkout")}
            className="retry-button"
          >
            Try Again
          </button>
          <button onClick={() => navigate("/")} className="home-button">
            Continue Shopping
          </button>
        </div>
      </div>
    </div>
  );
}

export default CheckoutCancelled;
