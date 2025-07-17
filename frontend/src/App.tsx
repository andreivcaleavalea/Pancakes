import React from "react";
import { ConfigProvider } from "antd";
import { RouterProvider } from "./router/RouterProvider";
import MainLayout from "./layouts/MainLayout/MainLayout";
import PageRenderer from "./router/PageRenderer";
import "./App.css";

const App: React.FC = () => {
  return (
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: "#e9833a",
          borderRadius: 8,
          colorBgContainer: "#ffffff",
        },
        components: {
          Button: {
            borderRadiusLG: 8,
            controlHeight: 40,
          },
          Input: {
            borderRadius: 8,
            controlHeight: 40,
          },
        },
      }}
    >
      <RouterProvider>
        <MainLayout>
          <PageRenderer />
        </MainLayout>
      </RouterProvider>
    </ConfigProvider>
  );
};

export default App;
