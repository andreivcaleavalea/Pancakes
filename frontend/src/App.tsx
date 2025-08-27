import React from "react";
import { ConfigProvider, App as AntApp } from "antd";
import { RouterProvider } from "./router/RouterProvider";
import MainLayout from "./layouts/MainLayout/MainLayout";
import PageRenderer from "./router/PageRenderer";

import { SavedBlogsProvider } from "./contexts/SavedBlogsContext";
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
      <AntApp>
        <SavedBlogsProvider>
          <RouterProvider>
            <MainLayout>
              <PageRenderer />
            </MainLayout>
          </RouterProvider>
        </SavedBlogsProvider>
      </AntApp>
    </ConfigProvider>
  );
};

export default App;
