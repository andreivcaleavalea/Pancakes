import React from "react";
import { ConfigProvider } from "antd";
import MainLayout from "./layouts/MainLayout/MainLayout";
import { RouterProvider } from "./router/RouterProvider";
import PageRenderer from "./router/PageRenderer";
import "./styles/globals.scss";
import "./App.css";

const theme = {
  token: {
    colorPrimary: "#6840C6",
    fontFamily:
      '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif',
  },
};

const App: React.FC = () => {
  return (
    <ConfigProvider theme={theme}>
      <RouterProvider>
        <MainLayout>
          <PageRenderer />
        </MainLayout>
      </RouterProvider>
    </ConfigProvider>
  );
};

export default App;
