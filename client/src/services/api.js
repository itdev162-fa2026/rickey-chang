const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL || "http://localhost:5161";

// ... (keep existing getProducts, getProductById, searchProducts functions)

// Create Stripe checkout session
export const createCheckoutSession = async (customerEmail, cartItems) => {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/checkout/create-session`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          customerEmail,
          items: cartItems.map((item) => ({
            productId: item.product.id,
            quantity: item.quantity,
          })),
        }),
      }
    );

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || "Failed to create checkout session");
    }

    return await response.json();
  } catch (error) {
    console.error("Error creating checkout session:", error);
    throw error;
  }
};

// Get order by Stripe session ID
export const getOrderBySessionId = async (sessionId) => {
  try {
    const response = await fetch(
      `${API_BASE_URL}/api/orders/session/${sessionId}`
    );

    if (!response.ok) {
      throw new Error("Failed to fetch order");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching order:", error);
    throw error;
  }
};
